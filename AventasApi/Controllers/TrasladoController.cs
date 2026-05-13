using ApiTrasladoService.Shared.Utils;
using ApiTrasladoService.Traslado.Models.DTOs;
using ApiTrasladoService.Traslado.Models.Enum;
using ApiTrasladoService.Traslado.Models.XmlDtos;
using AventasApi.Models.Traslado.DTOs;
using AventasApi.Models.Traslado.XmlDtos;
using DBData.Database;
using ExternalApiData.Enviroments;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;
using OfficeOpenXml.Style;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Serialization;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/traslado")]
    public class TrasladoController : ApiController
    {


        [HttpPost]
        [Route("postEnviarExcelCorreo")]
        public async Task<IHttpActionResult> PostEnviarExcelCorreo([FromBody] SendTrasladoRequestDto body)
        {
            try
            {
                var respuesta = await this.EnviarExcelCorreo(body);
                return Ok(respuesta);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        [Route("postSincronizarPlantillaAX")]

        public async Task<IHttpActionResult> PostSincronizarPlantillaAX()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Content(HttpStatusCode.UnsupportedMediaType, "Debe ser multipart/form-data");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Archivo
            var fileContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "archivo");
            if (fileContent == null)
            {
                return BadRequest("Archivo no proporcionado");
            }
            var bytes = await fileContent.ReadAsByteArrayAsync();
            var fileName = fileContent.Headers.ContentDisposition.FileName.Trim('\"');

            // Campos
            var nombreDelVendedor = await provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "nombreDelVendedor")
                ?.ReadAsStringAsync();

            var codigoDelVendedor = await provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "codigoDelVendedor")
                ?.ReadAsStringAsync();

            var company = await provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "company")
                ?.ReadAsStringAsync();

            var correoUsuario = await provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('\"') == "correoUsuario")
                ?.ReadAsStringAsync();

            // Armo el DTO manualmente
            var request = new SincronizarPlantillaAXRequestDto
            {
                Archivo = new MemoryPostedFile(bytes, fileName, fileContent.Headers.ContentType?.MediaType ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
                NombreDelVendedor = nombreDelVendedor,
                CodigoDelVendedor = codigoDelVendedor,
                Company = company,
                CorreoUsuario = correoUsuario
            };


            try
            {
                var respuesta = await SincronizarPlantillaAX(request);
                return Ok(respuesta);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private async Task<TrasladoResponseDto> EnviarExcelCorreo(SendTrasladoRequestDto body)
        {
            TrasladoResponseDto response = new TrasladoResponseDto
            {
                isComplete = true,
                message = ""
            };

            try
            {

                IMTrasladoPedidoDto detalleDelPedido = new IMTrasladoPedidoDto();
                detalleDelPedido.Encabezado = await this.TrasladoPedidoEncabezado(body.pedido, body.dataAreaId);
                if (detalleDelPedido.Encabezado != null &&
                    detalleDelPedido.Encabezado.SALESSTATUS != (int)SalesStatusEnum.PedidoAbierto)
                {
                    response.isComplete = false;
                    response.message = $"El pedido {body.pedido} no se encuentra en estado 'Pedido abierto'. No es posible realizar un traslado en su estado actual.";
                    return response;
                }
                else if (detalleDelPedido.Encabezado == null)
                {
                    response.isComplete = false;
                    response.message = $"No se encontró el pedido {body.pedido}.";
                    return response;
                }



                detalleDelPedido.Motivos = await this.TrasladoPedidoMotivo(body.dataAreaId);
                detalleDelPedido.Lineas = await this.TrasladoPedidolLineas(body.pedido, body.dataAreaId);


                // Generar Excel
                byte[] archivoExcel = await GenerarExcelTraslado(body.pedido, detalleDelPedido);

                using (var ctx = new AVentasEntities())
                {
                    // Preparar correo
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "CorreoGira").Valor);
                    mail.To.Add(body.emailDestino);

                    mail.Subject = $"Traslado de Pedido {body.pedido}";
                    mail.Body = "Se adjunta el archivo de distribución del pedido.";
                    mail.IsBodyHtml = false;

                    using (MemoryStream ms = new MemoryStream(archivoExcel))
                    {
                        string fileName = $"Traslado_{body.pedido}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        Attachment attachment = new Attachment(ms, fileName,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        mail.Attachments.Add(attachment);

                        // Configurar SMTP
                        SmtpClient smtp = new SmtpClient("smtp.office365.com", 587)
                        {
                            EnableSsl = true,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "CorreoGira").Valor, ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "PasswordCorreoGira").Valor)
                        };

                        smtp.Send(mail);
                        smtp.Dispose();
                    }
                }

                response.message = "Plantilla fue enviada al correo: " + body.emailDestino;
            }
            catch (Exception ex)
            {
                response.isComplete = false;
                response.message = $"Error al enviar correo: {ex.InnerException.Message}";
            }

            return response;
        }

        private async Task<byte[]> GenerarExcelTraslado(string pedidoOrigen, IMTrasladoPedidoDto detallesDelPedido)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Traslado");

                // Cabecera
                worksheet.Cells["B1"].Value = "Pedido Origen";
                worksheet.Cells["C1"].Value = pedidoOrigen;
                worksheet.Cells["C1"].Style.Font.Bold = true;
                worksheet.Cells["B2"].Value = "Lote";
                worksheet.Cells["C2"].Value = detallesDelPedido.Encabezado.BFPSEASONID;
                worksheet.Cells["B3"].Value = "Motivo";

                worksheet.Cells["C1:H1"].Merge = true;
                worksheet.Cells["C2:H2"].Merge = true;
                worksheet.Cells["C3:H3"].Merge = true;

                var rangoOne = worksheet.Cells["B1:H1"];
                rangoOne.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangoOne.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangoOne.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangoOne.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                var rangoTwo = worksheet.Cells["B2:H2"];
                rangoTwo.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangoTwo.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangoTwo.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangoTwo.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                var rangoThree = worksheet.Cells["B3:H3"];
                rangoThree.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rangoThree.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rangoThree.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rangoThree.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                var motivosSheet = package.Workbook.Worksheets.Add("Motivos");

                for (int i = 0; i < detallesDelPedido.Motivos.Count; i++)
                {
                    var description = detallesDelPedido.Motivos[i].Description?.Replace(",", "");
                    motivosSheet.Cells[i + 1, 1].Value = description.Trim();

                }
                int lastRow = detallesDelPedido.Motivos.Count;
                string rangoMotivos = $"Motivos!$A$1:$A${lastRow}";

                var validation = worksheet.DataValidations.AddListValidation("C3");
                validation.Formula.ExcelFormula = rangoMotivos;


                // Encabezados de tabla
                int startCol = 2; // Columna B

                worksheet.Cells[5, startCol, 6, startCol].Merge = true;
                worksheet.Cells[5, startCol].Value = "Articulo";

                worksheet.Cells[5, startCol + 1, 6, startCol + 1].Merge = true;
                worksheet.Cells[5, startCol + 1].Value = "Color";

                worksheet.Cells[5, startCol + 2, 6, startCol + 2].Merge = true;
                worksheet.Cells[5, startCol + 2].Value = "Talla";

                worksheet.Cells[5, startCol + 3, 6, startCol + 3].Merge = true;
                worksheet.Cells[5, startCol + 3].Value = "Cantidad Origen";


                worksheet.Cells["B5:E5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["B5:E5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["K5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["K5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["B5:E5"].Style.WrapText = true;

                // Subencabezados de cuentas
                for (int i = 0; i < 5; i++)
                {
                    worksheet.Cells[5, startCol + 4 + i].Value = $"Cuenta Destino {i + 1}";
                    worksheet.Cells[5, startCol + 4 + i].Style.Font.Bold = true;
                }

                for (int i = 0; i < 5; i++)
                {
                    worksheet.Cells[6, startCol + 4 + i].Value = $"";
                    worksheet.Cells[6, startCol + 4 + i].Style.Font.Bold = true;
                };

                // Total pendiente (columna final)
                worksheet.Cells[5, startCol + 9, 6, startCol + 9].Merge = true;
                worksheet.Cells[5, startCol + 9].Value = "Total Pendiente";
                worksheet.Cells[5, startCol + 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[5, startCol + 9].Style.Font.Bold = true;

                // Estilo para los encabezados anteriores
                for (int i = 0; i <= 3; i++)
                {
                    worksheet.Cells[6, startCol + i].Value = ""; // fila vacía debajo
                    worksheet.Cells[5, startCol + i].Style.Font.Bold = true;
                }

                // Datos
                int row = 7;
                foreach (var item in detallesDelPedido.Lineas)
                {
                    var celdaItem = worksheet.Cells[row, 2];
                    celdaItem.Value = item.ITEMID;
                    celdaItem.Style.Numberformat.Format = "@";

                    var celdaColor = worksheet.Cells[row, 3];
                    celdaColor.Value = item.INVENTCOLORID;
                    celdaColor.Style.Numberformat.Format = "@";

                    var celdaTalla = worksheet.Cells[row, 4];
                    celdaTalla.Value = item.INVENTSIZEID;
                    celdaTalla.Style.Numberformat.Format = "@";
                    celdaTalla.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    celdaTalla.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;



                    var celdaCantidad = worksheet.Cells[row, 5];
                    celdaCantidad.Style.Numberformat.Format = "0";
                    celdaCantidad.Value = item.REMAININVENTPHYSICAL;
                    celdaCantidad.Style.Font.Bold = true;
                    celdaCantidad.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    celdaCantidad.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    //Campos de cuenta destino

                    worksheet.Cells[row, 6].Style.Numberformat.Format = "0";
                    worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells[row, 7].Style.Numberformat.Format = "0";
                    worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    worksheet.Cells[row, 8].Style.Numberformat.Format = "0";
                    worksheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    worksheet.Cells[row, 9].Style.Numberformat.Format = "0";
                    worksheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    worksheet.Cells[row, 10].Style.Numberformat.Format = "0";
                    worksheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    //Total Pendiente
                    //Formula
                    var totalPendiente = $"=E{row}-(F{row}+G{row}+H{row}+I{row}+J{row})";

                    var celdaTotalPendiente = worksheet.Cells[row, 11];
                    celdaTotalPendiente.Formula = totalPendiente;
                    celdaTotalPendiente.Style.Numberformat.Format = "0";
                    celdaTotalPendiente.Style.Font.Bold = true;
                    celdaTotalPendiente.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    celdaTotalPendiente.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                    ///Regla 
                    var celda = worksheet.Cells[row, 11];
                    var rango = $"K{row}";

                    // Regla 1: valor == 0 → Verde
                    var reglaVerde = worksheet.ConditionalFormatting.AddEqual(celda);
                    reglaVerde.Formula = "0";
                    reglaVerde.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    reglaVerde.Style.Fill.BackgroundColor.Color = System.Drawing.Color.LightGreen;

                    // Regla 2: valor < 0 → Rojo
                    var reglaRojo = worksheet.ConditionalFormatting.AddLessThan(celda);
                    reglaRojo.Formula = "0";
                    reglaRojo.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    reglaRojo.Style.Fill.BackgroundColor.Color = System.Drawing.Color.LightSalmon;

                    // Regla 3: valor > 0 → Naranja
                    var reglaNaranja = worksheet.ConditionalFormatting.AddGreaterThan(celda);
                    reglaNaranja.Formula = "0";
                    reglaNaranja.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    reglaNaranja.Style.Fill.BackgroundColor.Color = System.Drawing.Color.FromArgb(255, 229, 153); ;

                    for (int col = 6; col <= 10; col++) // Columnas F (6) a J (10)
                    {
                        var celdaRule = worksheet.Cells[row, col];
                        var letraColumna = ((char)('A' + col - 1)).ToString(); // F, G, H, I, J

                        var regla = worksheet.ConditionalFormatting.AddExpression(celdaRule);
                        regla.Formula = $"{letraColumna}{row}>E{row}";

                        regla.Style.Border.BorderAround(ExcelBorderStyle.Thick, System.Drawing.Color.Red);
                    }

                    row++;
                }


                var startCell = worksheet.Cells[5, 2];
                var endCell = worksheet.Cells[row - 1, 2 + 9];
                var dataRange = worksheet.Cells[startCell.Start.Row, startCell.Start.Column, endCell.End.Row, endCell.End.Column];

                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                worksheet.Cells.AutoFitColumns();
                worksheet.Column(11).Width = 25;
                worksheet.Column(5).Width = 25;

                //Freeze
                worksheet.View.FreezePanes(7, 1);

                //Barra de datos
                var celdaResultado = worksheet.Cells["K2"];
                celdaResultado.Formula = $"=(SUM(K7:K${(row - 1)})/SUM(E7:E${(row - 1)})-1)*-1";

                // Formato condicional: barra de datos
                var condFormat = celdaResultado.ConditionalFormatting.AddDatabar(System.Drawing.Color.LightGreen);

                condFormat.LowValue.Type = eExcelConditionalFormattingValueObjectType.Num;
                condFormat.LowValue.Value = 0;

                condFormat.HighValue.Type = eExcelConditionalFormattingValueObjectType.Num;
                condFormat.HighValue.Value = 1;

                celdaResultado.Style.Font.Bold = true;
                celdaResultado.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                celdaResultado.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                celdaResultado.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                celdaResultado.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                celdaResultado.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                celdaResultado.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                celdaResultado.Style.Numberformat.Format = "0%";

                condFormat.ShowValue = true;
                condFormat.Gradient = true;


                return await Task.FromResult(package.GetAsByteArray());
            }
        }

        public async Task<TrasladoResponseDto> SincronizarPlantillaAX(SincronizarPlantillaAXRequestDto request)
        {
            TrasladoResponseDto response = new TrasladoResponseDto
            {
                isComplete = true,
                message = ""
            };
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var package = new ExcelPackage(request.Archivo.InputStream);
            var hoja = package.Workbook.Worksheets[0];

            //Bases del encabezado
            var salesId = hoja.Cells["C1"].Text;
            var lote = hoja.Cells["C2"].Text;
            var motivo = hoja.Cells["C3"].Text;

            //Validar pedidos de traslado ya fueron creados.
            var isExistTraslados = await this.ExisteTrasladoParaPedido(salesId, request.Company);
            if (isExistTraslados)
            {
                response.isComplete = false;
                response.message = $"Ya existen pedidos de traslado para {salesId}";
                return response;
            }

            //Comparasion del pedido actual con la BD a ver si se parecen.

            //Lineas en el excel

            var filaActual = 7;
            List<IMTrasladoPedidoLineaDto> lineasInFile = new List<IMTrasladoPedidoLineaDto>();

            while (true)
            {
                var itemId = hoja.Cells[$"B{filaActual}"].Text.Trim();
                var inventColorId = hoja.Cells[$"C{filaActual}"].Text.Trim();
                var inventSizeId = hoja.Cells[$"D{filaActual}"].Text.Trim();
                var remainInventPhysical = hoja.Cells[$"E{filaActual}"].Text.Trim();

                if (string.IsNullOrWhiteSpace(itemId))
                    break;

                lineasInFile.Add(new IMTrasladoPedidoLineaDto
                {
                    ITEMID = itemId,
                    INVENTCOLORID = inventColorId,
                    INVENTSIZEID = inventSizeId,
                    REMAININVENTPHYSICAL = decimal.Parse(remainInventPhysical)
                });

                filaActual++;
            };

            var lineasInDB = await this.TrasladoPedidolLineas(salesId, request.Company);
            var lineasDbDict = lineasInDB
                .GroupBy(l => $"{l.ITEMID}|{l.INVENTCOLORID}|{l.INVENTSIZEID}")
                .ToDictionary(g => g.Key, g => g.First());

            //Validar que las lineas tengan el mismo count
            if (lineasInFile.Count() != lineasInDB.Count())
            {
                response.isComplete = false;
                response.message = $"Cantidad de líneas no coincide: Archivo={lineasInFile.Count}, AX={lineasDbDict.Count}. Regenera el archivo y verifica los datos.";
                return response;
            }

            //Validar líneas nuevas o diferentes
            int lineaFile = 7;
            foreach (var l in lineasInFile)
            {
                var key = $"{l.ITEMID}|{l.INVENTCOLORID}|{l.INVENTSIZEID}";

                if (!lineasDbDict.TryGetValue(key, out var dbLinea))
                {
                    response.isComplete = false;
                    response.message = $"Línea {lineaFile}: El artículo del Excel no existe en el pedido de AX. Artículo: {l.ITEMID}, Color: {l.INVENTCOLORID}, Talla: {l.INVENTSIZEID}, Cantidad = {l.REMAININVENTPHYSICAL}.";
                    return response;
                }

                if (l.REMAININVENTPHYSICAL != dbLinea.REMAININVENTPHYSICAL)
                {
                    response.isComplete = false;
                    response.message = $"Línea {lineaFile}: Diferencia de cantidad entre Excel y AX. Artículo: {l.ITEMID}, Color: {l.INVENTCOLORID}, Talla: {l.INVENTSIZEID}. Excel = {l.REMAININVENTPHYSICAL}, AX = {(dbLinea.REMAININVENTPHYSICAL).ToString("0")}.";
                    return response;
                }

                lineaFile++;
            }


            //Aramado de XML
            //F G H I J
            var columnasClientes = new[] { 6, 7, 8, 9, 10 };
            var xmlList = new List<string>();

            foreach (var col in columnasClientes)
            {
                string cuentaCliente = hoja.Cells[6, col].Text.Trim();
                if (string.IsNullOrWhiteSpace(cuentaCliente))
                    continue;


                var customer = await this.TrasladoPedidoCustomer(cuentaCliente, request.Company);

                if (customer == null)
                {
                    response.isComplete = false;
                    response.message = $"El cliente {cuentaCliente} no fue encontrado para la compañia {request.Company}.";
                    return response;
                }

                var lineas = new List<TrasladoLineaDto>();
                int fila = 7;

                while (true)
                {
                    var itemId = hoja.Cells[$"B{fila}"].Text.Trim();
                    if (string.IsNullOrWhiteSpace(itemId))
                        break;

                    var cantidadTexto = hoja.Cells[fila, col].Text;

                    if (!string.IsNullOrWhiteSpace(cantidadTexto) && int.TryParse(cantidadTexto, out var cantidad))
                    {

                        if (cantidad < 0)
                        {
                            response.isComplete = false;
                            response.message = $"Error en fila {fila}: la cantidad disponible es negativa para el artículo '{itemId}' (valor: {cantidad}).";
                            return response;
                        }

                        var linea = new TrasladoLineaDto
                        {
                            ItemId = itemId,
                            InventColorId = hoja.Cells[$"C{fila}"].Text.Trim(),
                            InventSizeId = hoja.Cells[$"D{fila}"].Text.Trim(),
                            qty = cantidad
                        };

                        lineas.Add(linea);
                    }

                    fila++;
                }

                if (lineas.Count > 0)
                {
                    var encabezado = new TrasladoEncabezadoDto
                    {
                        PedidoOrigen = salesId,
                        Lote = lote,
                        Motivo = motivo,
                        CuentaDeCliente = cuentaCliente,
                        NombreDelVendedor = request.NombreDelVendedor,
                        CodigoDelVendedor = request.CodigoDelVendedor
                    };

                    var trasladoLineas = new TrasladoLineasDto
                    {
                        Lineas = lineas.ToArray()
                    };

                    var headerSerialize = XmlSerialize.SerializeToXmlWithoutDeclaration(encabezado);
                    var lineasSerialize = XmlSerialize.SerializeToXmlWithoutDeclaration(trasladoLineas);

                    string clienteXml = $@"<Cliente>{headerSerialize}{lineasSerialize}</Cliente>";

                    xmlList.Add(clienteXml);
                }
            }

            // Validar que Total Pendiente (columna K) sea cero para todas las filas activas
            bool totalPendienteValido = true;
            var filaPendiente = 7;

            while (true)
            {
                var itemId = hoja.Cells[$"B{filaPendiente}"].Text.Trim();
                var color = hoja.Cells[$"C{filaPendiente}"].Text.Trim();
                var talla = hoja.Cells[$"D{filaPendiente}"].Text.Trim();
                if (string.IsNullOrWhiteSpace(itemId))
                    break;

                var pendienteTexto = hoja.Cells[filaPendiente, 11].Text?.Trim(); // Columna K = 11

                if (!string.IsNullOrWhiteSpace(pendienteTexto) && pendienteTexto != "0" && pendienteTexto != "0.0")
                {
                    totalPendienteValido = false;
                    response.isComplete = false;
                    response.message = $"Error: El total pendiente en la fila {filaPendiente} ({itemId} - {color} - {talla}) no es cero (valor: '{pendienteTexto}'). Verifica que todas las cantidades estén distribuidas correctamente.";
                    return response;
                }

                filaPendiente++;
            }

            string xmlFinal = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><Clientes>{string.Join("", xmlList)}</Clientes>";


            var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}traslado/insertTraslado");
            client.Timeout = 480 * (1000);

            var requestPost = new RestRequest(Method.POST);
            requestPost.AddHeader("Accept", "application/json");
            requestPost.AddJsonBody(new
            {
                empresaAx = request.Company,
                clienteXml = xmlFinal,
            });

            var responseExec = client.Execute<string>(requestPost);

            if (responseExec.IsSuccessful)
            {
                response.isComplete = responseExec.Data.Contains("OK:");
                response.message = responseExec.Data.Replace("OK:", "").Trim();

                if (response.isComplete)
                {

                    try
                    {

                        await this.NewRegistroDeTraslado(salesId, response.message, request.Company, request.CodigoDelVendedor);
                        await this.EnvioDeCoreoDeTraslados(xmlFinal, salesId, request.Company, response.message, request.CorreoUsuario);
                    }
                    catch (Exception)
                    {
                        response.message = response.message + "\n Traslados creados correctamente, pero hubo un problema al enviar el correo de notificacion.";
                        return response;
                    }

                }
                return response;
            }
            else
            {
                response.isComplete = false;
                response.message = responseExec.Data;
                return response;
            }

        }


        private async Task<List<IMTrasladoPedidoMotivoDTO>> TrasladoPedidoMotivo(string dataAreaId)
        {
            List<IMTrasladoPedidoMotivoDTO> listaMotivo = new List<IMTrasladoPedidoMotivoDTO>();

            var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}traslado/motivo/{dataAreaId}");
            client.Timeout = 480 * (1000);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");

            var response = client.Execute<List<string>>(request);

            foreach (var item in response.Data)
            {
                listaMotivo.Add(new IMTrasladoPedidoMotivoDTO
                {
                    Description = item,
                    SalesOrders = true
                });
            }

            return listaMotivo;

        }


        private async Task<IMTrasladoPedidoEncabezadoDto> TrasladoPedidoEncabezado(string salesId, string dataAreaId)
        {
            var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}traslado/encabezado/{salesId}/{dataAreaId}");
            client.Timeout = 480 * (1000);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");

            var response = client.Execute<IMTrasladoPedidoEncabezadoDto>(request);

            return response.Data.SALESID != string.Empty ? response.Data : null;
        }

        private async Task<IMTrasladoPedidoCustomerDto> TrasladoPedidoCustomer(string accountNum, string dataAreaId)
        {
            var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}traslado/customer/{accountNum}/{dataAreaId}");
            client.Timeout = 480 * (1000);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");

            var response = client.Execute<IMTrasladoPedidoCustomerDto>(request);

            return response.Data.ACCOUNTNUM != string.Empty ? response.Data : null;
        }

        private async Task<List<IMTrasladoPedidoLineaDto>> TrasladoPedidolLineas(string salesId, string dataAreaId)
        {
            var client = new RestClient($"{Enviroment.CRMWebServiceURLApi}traslado/lineas/{salesId}/{dataAreaId}");
            client.Timeout = 480 * (1000);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");

            var response = client.Execute<List<IMTrasladoPedidoLineaDto>>(request);

            return response.Data;
        }

        private async Task<IHttpActionResult> NewRegistroDeTraslado(string pedidoBase, string msgSucces, string dataAreaId, string codigoDelVendedor)
        {

            var listaDeTraslados = Regex.Matches(msgSucces, @"'(?<pv>PV-\d+)'")
                                    .Cast<Match>()
                                    .Select(m => m.Groups["pv"].Value)
                                    .ToList();


            try
            {
                using (AVentasEntities context = new AVentasEntities())
                {
                    for (int i = 0; i < listaDeTraslados.Count(); i++)
                    {
                        string itemTraslado = listaDeTraslados[i];

                        var nuevoRegistro = new RegistroDeTraslado
                        {
                            PedidoBase = pedidoBase,
                            PedidoTraslado = itemTraslado,
                            CreadoPor = codigoDelVendedor,
                            FechaDeCreacion = DateTime.Now,
                            Company = dataAreaId
                        };

                        context.RegistroDeTraslado.Add(nuevoRegistro);
                        await context.SaveChangesAsync();
                    }

                    return Ok();
                }
            }
            catch (Exception e)
            {

                return BadRequest(e.ToString());
            }

        }

        private async Task<bool> ExisteTrasladoParaPedido(string pedidoBase, string company)
        {
            using (var context = new AVentasEntities())
            {

                var traslados = await context.RegistroDeTraslado
                                             .AsNoTracking()
                                             .Where(x => x.PedidoBase == pedidoBase && x.Company == company)
                                             .Select(x => new { x.PedidoTraslado, x.Company })
                                             .ToListAsync();

                // Si no hay más de uno no bloquea
                if (traslados.Count <= 0)
                    return false;
                bool todosCancelados = false;

                // Recorre y corta en cuanto encuentre uno NO cancelado
                foreach (var t in traslados)
                {
                    var pedido = await this.TrasladoPedidoEncabezado(t.PedidoTraslado, t.Company);

                    // BLOQUEA
                    if (pedido == null || pedido.SALESSTATUS != (int)SalesStatusEnum.Cancelado)
                    {
                        todosCancelados = true;
                        break;
                    }
                }


                return todosCancelados;
            }
        }

        private async Task EnvioDeCoreoDeTraslados(string xmlFinal, string pedidoBase, string company, string messageResponse, string emailAsesor)
        {
            // 1. Deserializar el XML a objetos
            XmlSerializer serializer = new XmlSerializer(typeof(ClientesDto));
            ClientesDto clientes;
            using (StringReader reader = new StringReader(xmlFinal))
            {
                clientes = (ClientesDto)serializer.Deserialize(reader);
            }

            // 2. Obtener líneas del pedido base

            List<IMTrasladoPedidoLineaDto> lineasPedidoBase = await this.TrasladoPedidolLineas(pedidoBase, company);
            IMTrasladoPedidoEncabezadoDto encabezadoPedidoBase = await this.TrasladoPedidoEncabezado(pedidoBase, company);

            List<ExtraerPedidosDto> listaPedidosMsg = ExtraerPedidos(messageResponse);

            // 3. Construir HTML
            var sb = new StringBuilder();
            sb.Append(@"
                <html>
                <head>
                    <style>
                        body { font-family: Arial, sans-serif; font-size: 14px; color: #333; }
                        h2 { margin-bottom: 0; }
                        table { border-collapse: collapse; width: 100%; margin-top: 10px; }
                        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                        th { background-color: #f2f2f2; }
                        .section { border: 1px solid #ccc; padding: 15px; margin-bottom: 20px; border-radius: 6px; }
                        .small { color: #777; font-size: 12px; }
                    </style>
                </head>
                <body>
                    <h2>Notificación de Traslado de Pedido</h2>
            ");

            //Resumen
            var clientesResumen = new List<(string Cuenta, string Nombre, string pedidoDeVenta, string OrigenTraslado)>();

            using (AVentasEntities context = new AVentasEntities())
            {
                sb.Append("<div class='section'>");
                sb.Append("<h3>Resumen</h3>");
                sb.Append("<table><tr><th>Tipo</th><th>Número de Pedido</th><th>Cuenta Cliente</th><th>Nombre Cliente</th><th>Total Cantidad</th></tr>");

                var cuentaBase = encabezadoPedidoBase.CUSTACCOUNT;

                var clienteBase = await context.Clientes
                    .FirstOrDefaultAsync(x =>
                        x.CodigoCliente == cuentaBase &&
                        x.EmpresaId == company);

                var nombreBase = clienteBase?.Nombre ?? "-";

                sb.Append("<tr>");
                sb.Append("<td>Origen</td>");
                sb.Append("<td>" + pedidoBase + "</td>");
                sb.Append("<td>" + cuentaBase + "</td>");

                int sumaBase = 0;

                foreach (var l in lineasPedidoBase)
                {
                    sumaBase += (int)l.REMAININVENTPHYSICAL;
                }

                sb.Append("<td>" + nombreBase + "</td>");
                sb.Append("<td>" + sumaBase + "</td>");
                sb.Append("</tr>");


                clientesResumen.Add((cuentaBase, nombreBase, pedidoBase, "O"));

                foreach (var cliente in clientes.Cliente)
                {
                    var cuentaCliente = cliente.Encabezado.CuentaDeCliente;

                    var nombreCliente = await context.Clientes
                        .FirstOrDefaultAsync(x =>
                            x.CodigoCliente == cuentaCliente &&
                            x.EmpresaId == company);

                    var nombre = nombreCliente?.Nombre ?? "-";


                    int sumaQty = 0;

                    foreach (var linea in cliente.Lineas.Lineas)
                    {
                        sumaQty += linea.qty;
                    }

                    var pedidoMsg = listaPedidosMsg
                        .FirstOrDefault(x => x.Cuenta == cuentaCliente);

                    clientesResumen.Add((cuentaCliente, nombre, pedidoMsg?.PV, "T"));

                    sb.Append("<tr>");
                    sb.Append("<td>Traslado</td>");
                    sb.Append("<td>" + (pedidoMsg?.PV ?? "-") + "</td>");
                    sb.Append("<td>" + cuentaCliente + "</td>");
                    sb.Append("<td>" + nombre + "</td>");
                    sb.Append("<td>" + sumaQty + "</td>");
                    sb.Append("</tr>");
                }

                sb.Append("</table>");
                sb.Append("</div>");
            }


            // Pedido Base
            sb.Append("<div class='section'>");
            sb.Append("<h3>Pedido Base</h3>");
            sb.Append("<p><strong>Número de Pedido:</strong> " + pedidoBase + "</p>");
            sb.Append("<p><strong>Cuenta Cliente:</strong> " + encabezadoPedidoBase.CUSTACCOUNT + "</p>");
            sb.Append("<p><strong>Lote:</strong> " + encabezadoPedidoBase.BFPSEASONID + "</p>");
            sb.Append("<table><tr><th>Código Artículo</th><th>Color</th><th>Talla</th><th>Cantidad</th></tr>");
            foreach (var l in lineasPedidoBase)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + l.ITEMID + "</td>");
                sb.Append("<td>" + l.INVENTCOLORID + "</td>");
                sb.Append("<td>" + l.INVENTSIZEID + "</td>");
                sb.Append("<td>" + l.REMAININVENTPHYSICAL + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            sb.Append("</div>");


            // Traslados realizados
            int trasladoNum = 1;


            foreach (var cliente in clientes.Cliente)
            {
                sb.Append("<div class='section'>");
                sb.Append("<h3>Traslado #" + trasladoNum + "</h3>");
                sb.Append("<p><strong>Pedido Destino:</strong> " + listaPedidosMsg.Find(x => x.Cuenta == cliente.Encabezado.CuentaDeCliente).PV + "</p>");
                sb.Append("<p><strong>Cliente Destino:</strong> " + cliente.Encabezado.CuentaDeCliente + "</p>");
                sb.Append("<p><strong>Lote:</strong> " + encabezadoPedidoBase.BFPSEASONID + "</p>");
                sb.Append("<table><tr><th>Código Artículo</th><th>Color</th><th>Talla</th><th>Cantidad</th></tr>");
                foreach (var linea in cliente.Lineas.Lineas)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>" + linea.ItemId + "</td>");
                    sb.Append("<td>" + linea.InventColorId + "</td>");
                    sb.Append("<td>" + linea.InventSizeId + "</td>");
                    sb.Append("<td>" + linea.qty + "</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</table>");
                sb.Append("</div>");
                trasladoNum++;
            }
            sb.Append("<p style='font-size:12px; color:#777; margin-top:20px;'>");
            sb.Append("Este es un mensaje automático del Sistema. Para consultas, contacte al departamento de créditos.");
            sb.Append("</p>");
            sb.Append("</body></html>");

            string htmlBody = sb.ToString();

            // 4. Enviar correo
            MailMessage mail = new MailMessage();

            using (AVentasEntities context = new AVentasEntities())
            {

                mail.From = new MailAddress(context.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "CorreoGira").Valor);

                var correos = context.Usuarios
                .Where(u =>
                    u.Usuario_Rol.Any(ur => ur.Roles.Nombre == "Seguimiento de traslado" && ur.status == true)
                )
                .Select(u => new { u.Id, u.Correo })
                .ToList();

                var correosUnicos = new List<string>();

                foreach (var correo in correos)
                {
                    if (!string.IsNullOrWhiteSpace(correo.Correo) && !correosUnicos.Contains(correo.Correo))
                    {
                        correosUnicos.Add(correo.Correo);
                    }
                }

                if (!string.IsNullOrWhiteSpace(emailAsesor))
                {
                    bool existe = correosUnicos.Contains(emailAsesor);

                    if (!existe)
                    {
                        mail.To.Add(emailAsesor);
                    }
                }

                foreach (var correo in correosUnicos)
                {
                    if (!string.IsNullOrWhiteSpace(correo))
                        mail.To.Add(correo);
                }

            }



            var traslados = clientesResumen
            .Where(x => x.OrigenTraslado == "T")
            .ToList();

            mail.Subject = traslados.Count == 1
                ? $"Traslado de Pedido {traslados[0].pedidoDeVenta} - {traslados[0].Nombre} "
                : $"Traslado de Pedido {pedidoBase}";
            mail.Body = htmlBody;
            mail.IsBodyHtml = true;

            using (var ctx = new AVentasEntities())
            {
                SmtpClient smtp = new SmtpClient("smtp.office365.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "CorreoGira").Valor, ctx.Configuraciones.FirstOrDefault(x => x.CodigoConfiguracion == "PasswordCorreoGira").Valor)
                };
                smtp.Send(mail);
            }
        }

        private static List<ExtraerPedidosDto> ExtraerPedidos(string mensaje)
        {
            var pedidos = new List<ExtraerPedidosDto>();

            // Regex para capturar PV y cualquier cuenta
            var patron = @"'(?<pv>PV-\d+)' para '(?<cuenta>[A-Z0-9\-]+)'";
            var matches = Regex.Matches(mensaje, patron);

            foreach (Match match in matches)
            {
                pedidos.Add(new ExtraerPedidosDto
                {
                    PV = match.Groups["pv"].Value,
                    Cuenta = match.Groups["cuenta"].Value
                });
            }

            return pedidos;
        }


    }
}