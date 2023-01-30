using AventasApi.enums;
using AventasApi.Models;
using AventasApi.Models.Authentication;
using AventasApi.Models.ViewModels;
using AventasApi.Services.Authentication;
using DBData.Database;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Interop;

namespace AventasApi.Controllers
{
    [RoutePrefix("api/trackingDevolucionCalidad")]
    public class TrackingDevolucionCalidadController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;

        public TrackingDevolucionCalidadController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }
    
        [HttpGet]
        [Route("obtenerDevolucionesAprobadas/{fechaInicio}/{fechaFin}/{bodegaEstado}/{asesor}")]
        public async Task<IHttpActionResult> ObtenerDevolucionesAprobadas( DateTime fechaInicio, DateTime fechaFin, int bodegaEstado, string asesor)
        {
          
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {

                    DateTime fechaFinHora = fechaFin.AddHours(24);

                    List<DevolucionesViewModel> devolucion = null;
                    var devolucionesAprovadas = ctx.AprobacionDevoluciones.Where(x => x.Aprobado == true &&  x.FechaCrea >= fechaInicio && x.FechaCrea <= fechaFinHora  ).Select(x => x.NumDevolucion).ToList();
                    if (bodegaEstado == 5)
                    {
                        devolucion = await ctx.Devolucion.Where(x => devolucionesAprovadas.Contains(x.NumDevolucion) && x.NumeroRMA != null && x.CodigoAsesor == asesor).Select(x => new DevolucionesViewModel
                        {
                            NumDevolucion = x.NumDevolucion,
                            NumeroRMA = x.NumeroRMA,
                            PedidoDevolucion = x.PedidoDevolucion,
                            CodigoCliente = x.CodigoCliente,
                            NombreCliente = x.Clientes.Nombre,
                            motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                            TotalUnidades = x.TotalUnidades,
                            Estado = x.Estado,
                            FechaCreacion = x.FechaCrea.Value,
                            SubTotal = x.Subtotal,
                            Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == x.CodigoAsesor).Nombre,
                            EstadoBodega = x.EstadoBodega,
                            Cliente = new ClienteViewModel
                            {
                                Direccion = x.Clientes.Direccion,
                                Moneda = x.Clientes.IdMoneda,
                                EmpresaId = x.Clientes.EmpresaId
                            },
                            //DevolucionesEstatus = new AprobacionDevolucionesViewModel
                            //{
                            //    Estado = true,
                            //}


                        }).ToListAsync();

                    }
                    else if(bodegaEstado ==3 ||bodegaEstado ==4) {

                        String estado = "";

                        if (bodegaEstado == 3) {
                            estado = "Pendiente Aprobacion";
                        } else
                        {
                            estado = "Aprobado";
                        }
                                                              
                        devolucion = await ctx.Devolucion.Where(x => devolucionesAprovadas.Contains(x.NumDevolucion) && x.Estado == estado  && x.CodigoAsesor == asesor).Select(x => new DevolucionesViewModel
                        {                        
                            NumDevolucion = x.NumDevolucion,
                            NumeroRMA = x.NumeroRMA,
                            PedidoDevolucion = x.PedidoDevolucion,
                            CodigoCliente = x.CodigoCliente,
                            NombreCliente = x.Clientes.Nombre,
                            motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                            TotalUnidades = x.TotalUnidades,
                            Estado = x.Estado,
                            FechaCreacion = x.FechaCrea.Value,
                            SubTotal = x.Subtotal,
                            Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == x.CodigoAsesor).Nombre,
                            EstadoBodega = x.EstadoBodega,
                            Cliente = new ClienteViewModel
                            {
                                Direccion = x.Clientes.Direccion,
                                Moneda = x.Clientes.IdMoneda,
                                EmpresaId = x.Clientes.EmpresaId
                            }
                        }).ToListAsync();
                    }
                    else {                  
                                                              
                        devolucion = await ctx.Devolucion.Where(x => devolucionesAprovadas.Contains(x.NumDevolucion) && x.EstadoBodega == bodegaEstado && x.NumeroRMA != null && x.CodigoAsesor == asesor).Select(x => new DevolucionesViewModel
                        {                        
                            NumDevolucion = x.NumDevolucion,
                            NumeroRMA = x.NumeroRMA,
                            PedidoDevolucion = x.PedidoDevolucion,
                            CodigoCliente = x.CodigoCliente,
                            NombreCliente = x.Clientes.Nombre,
                            motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                            TotalUnidades = x.TotalUnidades,
                            Estado = x.Estado,
                            FechaCreacion = x.FechaCrea.Value,
                            SubTotal = x.Subtotal,
                            Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == x.CodigoAsesor).Nombre,
                            EstadoBodega = x.EstadoBodega,
                            Cliente = new ClienteViewModel
                            {
                                Direccion = x.Clientes.Direccion,
                                Moneda = x.Clientes.IdMoneda,
                                EmpresaId = x.Clientes.EmpresaId
                            }
                        }).ToListAsync();
                    }

                    if (devolucion.Count == 0 && bodegaEstado != 3)
                    {
                        var estado =  Enum.GetName(typeof(EstadoBodega), bodegaEstado); ;

                      return estado != "Todo" ? BadRequest($"No se encuentran devoluciones entre {fechaInicio.ToString("yyyy/MM/dd")} y {fechaFin.ToString("yyyy/MM/dd")}") :
                            BadRequest($"No se encuentran devoluciones {estado.Replace("_", " ")} entre {fechaInicio.ToString("yyyy/MM/dd")} y {fechaFin.ToString("yyyy/MM/dd")}");
                    }

                    return Ok(devolucion);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpGet]
        [Route("obtenerDevolucionesAprobadas/{fechaInicio}/{fechaFin}/{bodegaEstado}")]
        public async Task<IHttpActionResult> ObtenerDevolucionesAprobadas(DateTime fechaInicio, DateTime fechaFin, int bodegaEstado)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    DateTime fechaFinHora = fechaFin.AddHours(24);
                    List<DevolucionesViewModel> devolucion = null;
                    var devolucionesAprovadas = ctx.AprobacionDevoluciones.Where(x => x.Aprobado == true && x.FechaCrea >= fechaInicio && x.FechaCrea <= fechaFinHora).Select(x => x.NumDevolucion).ToList();

                    if (bodegaEstado == 5) 
                    {
                        devolucion = await ctx.Devolucion.Where(x => devolucionesAprovadas.Contains(x.NumDevolucion)  && x.NumeroRMA != null).Select(x => new DevolucionesViewModel
                        {
                            NumDevolucion = x.NumDevolucion,
                            NumeroRMA = x.NumeroRMA,
                            PedidoDevolucion = x.PedidoDevolucion,
                            CodigoCliente = x.CodigoCliente,
                            NombreCliente = x.Clientes.Nombre,
                            motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                            TotalUnidades = x.TotalUnidades,
                            Estado = x.Estado,
                            FechaCreacion = x.FechaCrea.Value,
                            SubTotal = x.Subtotal,
                            Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == x.CodigoAsesor).Nombre,
                            EstadoBodega = x.EstadoBodega,
                            Cliente = new ClienteViewModel
                            {
                                Direccion = x.Clientes.Direccion,
                                Moneda = x.Clientes.IdMoneda,
                                EmpresaId = x.Clientes.EmpresaId
                            }
                        }).ToListAsync();
                    }
                    else
                    {
                        devolucion = await ctx.Devolucion.Where(x => devolucionesAprovadas.Contains(x.NumDevolucion) && x.NumeroRMA != null ).Select(x => new DevolucionesViewModel
                        {
                            NumDevolucion = x.NumDevolucion,
                            NumeroRMA = x.NumeroRMA,
                            PedidoDevolucion = x.PedidoDevolucion,
                            CodigoCliente = x.CodigoCliente,
                            NombreCliente = x.Clientes.Nombre,
                            motivoDevolucion = x.MotivosDevolucionDetalle.CodigoMotivoDevDetalle,
                            TotalUnidades = x.TotalUnidades,
                            Estado = x.Estado,
                            FechaCreacion = x.FechaCrea.Value,
                            SubTotal = x.Subtotal,
                            Usuario = ctx.Asesores.FirstOrDefault(ase => ase.CodigoAsesor == x.CodigoAsesor).Nombre,
                            EstadoBodega = x.EstadoBodega,
                            Cliente = new ClienteViewModel
                            {
                                Direccion = x.Clientes.Direccion,
                                Moneda = x.Clientes.IdMoneda,
                                EmpresaId = x.Clientes.EmpresaId
                            }
                        }).ToListAsync();

                    }

                    if (devolucion.Count == 0)
                    {
                        var estado = Enum.GetName(typeof(EstadoBodega), bodegaEstado); 
                        return estado != "Todo" ? BadRequest($"No se encuentran devoluciones entre {fechaInicio.ToString("yyyy/MM/dd")} y {fechaFin.ToString("yyyy/MM/dd")}") : 
                            BadRequest($"No se encuentran devoluciones {estado.Replace("_", " ")} entre {fechaInicio.ToString("yyyy/MM/dd")} y {fechaFin.ToString("yyyy/MM/dd")}");
                    }

                    return Ok(devolucion);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }



        [HttpPut]
        [Route("actualizarEstadoDevolucion/{numDevolucion}/{estadoBodega}")]
        public async Task<IHttpActionResult> ActualizarEstadoDevolucion(string numDevolucion, int estadoBodega)
        {
            using (AVentasEntities ctx = new AVentasEntities())
            {
                var result = 0;
                try {
                    var oldData = await ctx.Devolucion.FirstAsync(x => x.NumDevolucion == numDevolucion);
                    oldData.EstadoBodega = estadoBodega;
                    result = await ctx.SaveChangesAsync();

                    var usuarioData = await ctx.Usuarios.FirstOrDefaultAsync(x => x.usuario == oldData.CodigoAsesor);



                    if (result > 0 && usuarioData != null)
                    {
                        var correoPrincipal = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "CorreoPrincipal").FirstOrDefault();
                        var usuario = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "UsuarioCorreo").FirstOrDefault();
                        var password = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "CredencialCorreo").FirstOrDefault();
                        var port = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "MailPort").FirstOrDefault();
                        var host = ctx.Configuraciones.Where(x => x.CodigoConfiguracion == "Host").FirstOrDefault();
                        var estado = Enum.GetName(typeof(EstadoBodega), estadoBodega);
                                              
                        MailMessage mail = new MailMessage();
                        mail.IsBodyHtml = true;
                        mail.From = new MailAddress(correoPrincipal.Valor, usuario.Valor);
                        mail.To.Add(new MailAddress(usuarioData.Correo, usuarioData.usuario));
                        mail.Subject = $"Seguimiento de calidad devolucion {numDevolucion}";
                        mail.Body = $"<h1>Se cambio es estado de la devolucion {numDevolucion} a {estado.Replace("_", " ")} <h1>";

                        using (SmtpClient smtp = new SmtpClient())
                        {
                            smtp.Host = host.Valor;
                            smtp.Port = Convert.ToInt32(port.Valor);
                            smtp.EnableSsl = true;
                            System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                            NetworkCred.UserName = correoPrincipal.Valor;
                            NetworkCred.Password = password.Valor;
                            smtp.UseDefaultCredentials = true;
                            smtp.Credentials = NetworkCred;
                            smtp.Send(mail);
                        }
                   }
                }
                catch(Exception e)
                {

                    return BadRequest(e.ToString());

                }

                return Ok(result);
               
            }
        }

        [HttpGet]
        [Route("obtenerDetalleDevolucion/{numDevolucion}")]
        public IHttpActionResult ObtenerDetalleDevolucion(string numDevolucion)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    List<PedidosXClienteViewModel> devoluciones = ctx.Devolucion.Where(x => x.NumDevolucion == numDevolucion).Select(dev => new PedidosXClienteViewModel
                    {
                        gruposXDetPed = dev.DevolucionDetalle.GroupBy(gruposXDetPed => gruposXDetPed.ProductosxColeccion.CodigoGrupoTalla)
                        .Select(gruposXDetPed => new GruposTallaXDetPed
                        {
                            GrupoTalla = gruposXDetPed.Key,
                            ListaTalla = gruposXDetPed.GroupBy(pedDet => pedDet.CodigoTalla).Select(pedDet => pedDet.Key).SelectMany(pedDet => ctx.TallasXGrupo.Where(txp => txp.CodigoTalla.ToUpper().Trim() == pedDet.ToUpper().Trim() && txp.CodigoGrupoTalla.ToUpper().Trim() == gruposXDetPed.Key.ToUpper().Trim())).Select(txp => new TallaViewModel
                            {
                                GrupoTallaId = txp.CodigoGrupoTalla.ToUpper(),
                                Talla = txp.CodigoTalla.ToUpper(),
                                Orden = txp.Orden ?? 0,
                            }).OrderBy(txp => txp.Orden).ToList(),
                            prodsXDetPed = gruposXDetPed.GroupBy(pedDet => pedDet.IdProducto)
                            .Select(pedDet => new ProductosXDetPed
                            {
                                IdProducto = pedDet.Key,
                                CodigoProducto = pedDet.FirstOrDefault().ProductosxColeccion.CodigoProducto,
                                NombreProducto = pedDet.FirstOrDefault().ProductosxColeccion.NombreProducto,
                                Imagen = pedDet.FirstOrDefault().ProductosxColeccion.FotografiasXProducto.FirstOrDefault().FotografiaProducto,
                                CantidadXProducto = pedDet.Sum(cant => cant.Cantidad),
                                TotalXProducto = pedDet.Sum(cant => cant.MontoLinea),
                                coloresXProdXDetPed = pedDet.GroupBy(colXprod => colXprod.CodigoColor).Where(colXprod => colXprod.Sum(det => det.Cantidad) > 0).Select(colXprod =>
                                         new ColoresXProdXDetPed
                                         {
                                             CantidadXColor = colXprod.Sum(cant => cant.Cantidad),
                                             TotalXColor = colXprod.Sum(cant => cant.MontoLinea),
                                             PrecioXColor = colXprod.FirstOrDefault().PrecioUnitario,
                                             IdColor = colXprod.Key,
                                             NombreColor = ctx.Colores.FirstOrDefault(color => color.CodigoColor == colXprod.Key).Color,
                                             DetallesXPedido = colXprod.Select(detPed => new DetalleXPedidoViewModel
                                             {
                                                 IdRegistro = detPed.IdDevolucionDetalle,
                                                 PedidoId = detPed.NumDevolucion,
                                                 Cantidad = detPed.Cantidad,
                                                 MontoLinea = 0,
                                                 PrecioUnitario = detPed.PrecioUnitario,
                                                 Talla = detPed.CodigoTalla.ToUpper(),
                                                 TallaObject = ctx.TallasXGrupo.Where(txp => txp.CodigoGrupoTalla == detPed.ProductosxColeccion.CodigoGrupoTalla && txp.CodigoTalla == detPed.CodigoTalla).Select(txp => new TallaViewModel
                                                 {
                                                     GrupoTallaId = txp.CodigoGrupoTalla,
                                                     Talla = txp.CodigoTalla.ToUpper(),
                                                     Orden = txp.Orden ?? 0,
                                                 }).FirstOrDefault()
                                             }).ToList()

                                         }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList();

                    return Ok(devoluciones[0].gruposXDetPed);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpGet]
        [Route("reporte/{numDevolucion}")]
        public async Task<IHttpActionResult> GetReporteDevolucion(string numDevolucion)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var detalleDevolucion = await ctx.DevolucionDetalle.Where(x => x.NumDevolucion == numDevolucion).Select(x => new
                    {
                        Producto = x.ProductosxColeccion.CodigoProducto,
                        Color = x.CodigoColor,
                        Talla = x.CodigoTalla,
                        Cantidad = x.Cantidad,
                    }).ToListAsync();

                    return Ok(detalleDevolucion);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}
