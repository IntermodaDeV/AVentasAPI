using AventasApi.Models.ViewModels;
using DBData.Database;
using ExternalApiData.Enviroments;
using RestSharp;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class GiraAsesoresController : ApiController
    {
        [HttpGet]
        [Route("~/api/TipoGasto")]
        public async Task<IHttpActionResult> ObtenerTipoGasto()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var TipoGasto = await ctx.TipoGastoViaje.Select(x => new TipoGastoViewModel
                    {
                        Id = x.IdTipoGastoViaje,
                        Nombre = x.Nombre,
                        Diario = x.Diario,
                        Empresa = x.Empresa,
                        Activo = x.Activo
                    }).ToListAsync();
                    return Ok(TipoGasto);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/TipoGasto/{empresa}")]
        public async Task<IHttpActionResult> ObtenerTipoGasto(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var TipoGasto = await ctx.TipoGastoViaje.Where(x=>x.Empresa==empresa && x.Activo == true).Select(x => new TipoGastoViewModel
                    {
                        Id = x.IdTipoGastoViaje,
                        Nombre = x.Nombre
                    }).ToListAsync();
                    return Ok(TipoGasto);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/CategoriaGasto/{empresa}")]
        public async Task<IHttpActionResult> ObtenerCategoriaGasto(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var CategoriaGasto = await ctx.CategoriaTipoGastoViaje.
                        Join(ctx.TipoGastoViaje,
                            categoria => categoria.IdTipoGastoViaje,
                            tipo => tipo.IdTipoGastoViaje,
                            (categoria, tipo) => new { Categoria = categoria, Tipo = tipo })
                        .Where(x=> x.Tipo.Empresa == empresa)
                        .Select(x => new CategoriaGastoDetalleViewModel {
                            idCategoriaTipoGastoViaje = x.Categoria.IdCategoriaTipoGastoViaje,
                            IdTipoGastoViaje = x.Tipo.IdTipoGastoViaje,
                            TipoNombre = x.Tipo.Nombre,
                            Empresa = x.Tipo.Empresa,
                            CategoriaNombre = x.Categoria.Nombre,
                            ProveedorPredefinido = x.Categoria.ProveedorPredefinido,
                            CuentaContrapartida = x.Categoria.CuentaContrapartida,
                            FacturaObligatoria = x.Categoria.FacturaObligatoria,
                            Descripcion = x.Categoria.Descripcion,
                            imagen = x.Categoria.ImagenObligatoria,
                            activo = x.Categoria.Activo
                        })
                        

                    .ToListAsync();
                    return Ok(CategoriaGasto);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/GastosPendientes/{empresa}")]
        public async Task<IHttpActionResult> ObtenerGastosPendientes(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var pendientes = await ctx.GastosViajeDetalle
                        .Join(ctx.CategoriaTipoGastoViaje,
                            gasto => gasto.IdCategoriaTipoGastoViaje,
                            categoria => categoria.IdCategoriaTipoGastoViaje,
                            (gasto, categoria) => new { Gasto = gasto, Categoria = categoria })
                        .Join(ctx.TipoGastoViaje,
                            gastocategoria => gastocategoria.Categoria.IdTipoGastoViaje,
                            tipo => tipo.IdTipoGastoViaje,
                            (gastocategoria, tipo) => new { GastoCategoria = gastocategoria, Tipo = tipo }
                        )
                        .Where(x => x.GastoCategoria.Gasto.IdEstado == 1 && x.Tipo.Empresa == empresa)
                        .Select(x => new GastoPendienteViewModel
                        {
                            IdGastoViajeDetalle = x.GastoCategoria.Gasto.IdGastoViajeDetalle,
                            tipo = x.Tipo.Nombre,
                            categoria = x.GastoCategoria.Categoria.Nombre,
                            UsuarioAsesor = x.GastoCategoria.Gasto.UsuarioAsesor,
                            NoFactura = x.GastoCategoria.Gasto.NoFactura,
                            Descripcion = x.GastoCategoria.Gasto.Descripcion,
                            ValorFactura = x.GastoCategoria.Gasto.ValorFactura,
                            FechaFactura = x.GastoCategoria.Gasto.FechaFactura,
                            FechaCreacion = x.GastoCategoria.Gasto.FechaCreacion,
                            serie = x.GastoCategoria.Gasto.serie
                        })
                        .OrderBy(x=>x.FechaCreacion).ToListAsync();
                    return Ok(pendientes);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/GastosNoSincornizados/{empresa}")]
        public async Task<IHttpActionResult> ObtenerGastosNoSincronizados(string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var pendientes = await ctx.GastosViajeDetalle
                        .Join(ctx.CategoriaTipoGastoViaje,
                            gasto => gasto.IdCategoriaTipoGastoViaje,
                            categoria => categoria.IdCategoriaTipoGastoViaje,
                            (gasto, categoria) => new { Gasto = gasto, Categoria = categoria })
                        .Join(ctx.TipoGastoViaje,
                            gastocategoria => gastocategoria.Categoria.IdTipoGastoViaje,
                            tipo => tipo.IdTipoGastoViaje,
                            (gastocategoria, tipo) => new { GastoCategoria = gastocategoria, Tipo = tipo }
                        )
                        .Where(x => x.GastoCategoria.Gasto.IdEstado == 4 && x.Tipo.Empresa == empresa)
                        .Select(x => new GastoNoSincronizadosViewModel
                        {
                            IdGastoViajeDetalle = x.GastoCategoria.Gasto.IdGastoViajeDetalle,
                            tipo = x.Tipo.Nombre,
                            categoria = x.GastoCategoria.Categoria.Nombre,
                            UsuarioAsesor = x.GastoCategoria.Gasto.UsuarioAsesor,
                            serie = x.GastoCategoria.Gasto.serie,
                            NoFactura = x.GastoCategoria.Gasto.NoFactura,
                            Descripcion = x.GastoCategoria.Gasto.Descripcion,
                            MensajeAX = x.GastoCategoria.Gasto.MensajeAX,
                            ValorFactura = x.GastoCategoria.Gasto.ValorFactura,
                            FechaFactura = x.GastoCategoria.Gasto.FechaFactura,
                            FechaCreacion = x.GastoCategoria.Gasto.FechaCreacion
                        })
                        .OrderBy(x => x.FechaCreacion).ToListAsync();
                    return Ok(pendientes);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/HistorialGastos/{usuario}/{dateIni}/{dateFin}")]
        public async Task<IHttpActionResult> ObtenerHistorialGastos(string usuario, DateTime dateIni, DateTime dateFin)
        {
            dateFin = dateFin.AddHours(23);
            dateFin = dateFin.AddMinutes(59);

            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var pendientes = await ctx.GastosViajeDetalle
                        .Join(ctx.CategoriaTipoGastoViaje,
                            gasto => gasto.IdCategoriaTipoGastoViaje,
                            categoria => categoria.IdCategoriaTipoGastoViaje,
                            (gasto, categoria) => new { Gasto = gasto, Categoria = categoria })
                        .Join(ctx.TipoGastoViaje,
                            gastocategoria => gastocategoria.Categoria.IdTipoGastoViaje,
                            tipo => tipo.IdTipoGastoViaje,
                            (gastocategoria, tipo) => new { GastoCategoria = gastocategoria, Tipo = tipo }
                        )
                        .Join(ctx.Estado,
                            gastoEstado => gastoEstado.GastoCategoria.Gasto.IdEstado,
                            estado => estado.IdEstado,
                            (gastoEstado, Estado) => new { GastoEstado = gastoEstado, Estado = Estado }
                            )
                        .Where(x => x.GastoEstado.GastoCategoria.Gasto.UsuarioAsesor == usuario &&  
                          x.GastoEstado.GastoCategoria.Gasto.FechaCreacion  <= dateFin && 
                        x.GastoEstado.GastoCategoria.Gasto.FechaCreacion >= dateIni
                        && x.GastoEstado.GastoCategoria.Gasto.IdEstado != 1)
                        .Select(x => new HistorialGastosViewModel
                        {
                            IdGastoViajeDetalle = x.GastoEstado.GastoCategoria.Gasto.IdGastoViajeDetalle,
                            tipo = x.GastoEstado.Tipo.Nombre,
                            categoria = x.GastoEstado.GastoCategoria.Categoria.Nombre,
                            UsuarioAsesor = x.GastoEstado.GastoCategoria.Gasto.UsuarioAsesor,
                            NoFactura = x.GastoEstado.GastoCategoria.Gasto.NoFactura,
                            Descripcion = x.GastoEstado.GastoCategoria.Gasto.Descripcion,
                            DescripcionAdmin = x.GastoEstado.GastoCategoria.Gasto.DescripcionAdmin,
                            ValorFactura = x.GastoEstado.GastoCategoria.Gasto.ValorFactura,
                            FechaFactura = x.GastoEstado.GastoCategoria.Gasto.FechaFactura,
                            FechaCreacion = x.GastoEstado.GastoCategoria.Gasto.FechaCreacion,
                            Estado = x.Estado.Nombre,
                            serie = x.GastoEstado.GastoCategoria.Gasto.serie
                        }).ToListAsync();
                    return Ok(pendientes);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/GastosExcel/{usuario}/{dateIni}/{dateFin}")]
        public async Task<IHttpActionResult> ObtenerGastosExcel(string usuario, DateTime dateIni, DateTime dateFin)
        {
            dateFin = dateFin.AddHours(23);
            dateFin = dateFin.AddMinutes(59);

            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var excel = await ctx.GastosViajeDetalle
                        .Join(ctx.CategoriaTipoGastoViaje,
                            gasto => gasto.IdCategoriaTipoGastoViaje,
                            categoria => categoria.IdCategoriaTipoGastoViaje,
                            (gasto, categoria) => new { Gasto = gasto, Categoria = categoria })
                        .Join(ctx.TipoGastoViaje,
                            gastocategoria => gastocategoria.Categoria.IdTipoGastoViaje,
                            tipo => tipo.IdTipoGastoViaje,
                            (gastocategoria, tipo) => new { GastoCategoria = gastocategoria, Tipo = tipo }
                        )
                        .Join(ctx.Estado,
                            gastoEstado => gastoEstado.GastoCategoria.Gasto.IdEstado,
                            estado => estado.IdEstado,
                            (gastoEstado, Estado) => new { GastoEstado = gastoEstado, Estado = Estado }
                            )
                        .Where(x => x.GastoEstado.GastoCategoria.Gasto.UsuarioAsesor == usuario &&
                          x.GastoEstado.GastoCategoria.Gasto.FechaCreacion <= dateFin &&
                        x.GastoEstado.GastoCategoria.Gasto.FechaCreacion >= dateIni
                        && x.GastoEstado.GastoCategoria.Gasto.IdEstado == 2)
                        .Select(x => new GastosExcelViewModel
                        {
                            Tipo = x.GastoEstado.Tipo.Nombre,
                            categoria = x.GastoEstado.GastoCategoria.Categoria.Nombre,
                            descripcion = x.GastoEstado.GastoCategoria.Gasto.Descripcion,
                            fecha = x.GastoEstado.GastoCategoria.Gasto.FechaFactura,
                            valor = x.GastoEstado.GastoCategoria.Gasto.ValorFactura
                        }).ToListAsync();
                    return Ok(excel);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/GastoFotografia/{id}")]
        public async Task<IHttpActionResult> ObtenerHistorialGastos(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var imagen = await ctx.GastosViajeDetalle
                        .FirstOrDefaultAsync(x => x.IdGastoViajeDetalle == id);
                    return Ok(imagen.Imagen);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/DatosEnviarAX/{id}")]
        public async Task<IHttpActionResult> ObtenerDatosEnviarAX(int id)
        {
            
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var tmp = ctx.GastosViajeDetalle.FirstOrDefault(x => x.IdGastoViajeDetalle == id);
                    
                    if(tmp.IdEstado == 2)
                    {
                        var p = new { Content = "OK" };
                        
                        return Ok(p);
                    }

                    var datos = await ctx.GastosViajeDetalle
                        .Join(ctx.CategoriaTipoGastoViaje,
                            gasto => gasto.IdCategoriaTipoGastoViaje,
                            categoria => categoria.IdCategoriaTipoGastoViaje,
                            (gasto, categoria) => new { Gasto = gasto, Categoria = categoria })
                        .Join(ctx.TipoGastoViaje,
                            gastocategoria => gastocategoria.Categoria.IdTipoGastoViaje,
                            tipo => tipo.IdTipoGastoViaje,
                            (gastocategoria, tipo) => new { GastoCategoria = gastocategoria, Tipo = tipo }
                        )
                        .Join(ctx.MonedasxEmpresa,
                        gastoCategoriaTipo => gastoCategoriaTipo.Tipo.Empresa,
                        moneda => moneda.EmpresaId,
                        (gastoCategoriaTipo, moneda) => new { GastoCategoriaTipo = gastoCategoriaTipo, Moneda = moneda }
                        )
                        .Where(x => x.GastoCategoriaTipo.GastoCategoria.Gasto.IdGastoViajeDetalle == id && x.Moneda.IdMoneda != "USD")
                        .Select(x => new DatosAxViewModel
                        {
                            COMPANY = x.GastoCategoriaTipo.Tipo.Empresa,
                            CURRENCYCODE = x.Moneda.IdMoneda,
                            TRANSDATE = x.GastoCategoriaTipo.GastoCategoria.Gasto.FechaFactura.ToString().Substring(8, 2) + "/" + x.GastoCategoriaTipo.GastoCategoria.Gasto.FechaFactura.ToString().Substring(5, 2) +"/"+ x.GastoCategoriaTipo.GastoCategoria.Gasto.FechaFactura.ToString().Substring(0,4),
                            NUMBERINVOCEID = x.GastoCategoriaTipo.GastoCategoria.Gasto.NoFactura.Replace("-", "").Replace("-", "").Replace("-", ""),
                            DESCRIPTION = x.GastoCategoriaTipo.GastoCategoria.Gasto.DescripcionGasto,
                            CREDIT = x.GastoCategoriaTipo.GastoCategoria.Gasto.ValorFactura,
                            VENDACCOUNT = x.GastoCategoriaTipo.GastoCategoria.Gasto.Proveedor,
                            USERID = x.GastoCategoriaTipo.GastoCategoria.Gasto.UsuarioAsesor,
                            JOURNALNAME = x.GastoCategoriaTipo.Tipo.Diario,
                            OFFSETACCOUNT = x.GastoCategoriaTipo.GastoCategoria.Categoria.CuentaContrapartida,
                            SERIE = x.GastoCategoriaTipo.GastoCategoria.Gasto.serie
                        })
                        .Take(1).ToListAsync();

                    DatosAxViewModel datosAX = new DatosAxViewModel();
                    foreach(var dato in datos)
                    {
                        datosAX.COMPANY = dato.COMPANY;
                        datosAX.CURRENCYCODE = dato.CURRENCYCODE;
                        datosAX.TRANSDATE = dato.TRANSDATE;
                        datosAX.NUMBERINVOCEID = dato.NUMBERINVOCEID;
                        datosAX.DESCRIPTION = dato.DESCRIPTION;
                        datosAX.CREDIT = dato.CREDIT;
                        datosAX.VENDACCOUNT = dato.VENDACCOUNT;
                        datosAX.USERID = dato.USERID;
                        datosAX.JOURNALNAME = dato.JOURNALNAME;
                        datosAX.OFFSETACCOUNT = dato.OFFSETACCOUNT;
                        datosAX.SERIE = dato.SERIE;

                    }
                    var client = new RestClient();
                    var request = new RestRequest($"{Enviroment.CRMWebServiceURLApi}GiraAsesor/EnviarGasto", Method.POST)
                    {
                        RequestFormat = DataFormat.Json
                    };
                    request.AddHeader("Content-type", "application/json; charset=utf-8");
                    request.Parameters.Clear();
                    request.AddParameter("application/json", Newtonsoft.Json.JsonConvert.SerializeObject(datosAX), ParameterType.RequestBody);
                    var respuesta = client.Execute(request);
                    return Ok(respuesta);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        

        [HttpGet]
        [Route("~/api/empresas")]
        public async Task<IHttpActionResult> ObtenerEmpresa()
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var empresas = await ctx.Empresa.Select(x => new
                    {
                        Empresa = x.EmpresaId
                    }).ToListAsync();
                    return Ok(empresas);
                }
            }catch(Exception e)
            {
                return BadRequest(e.ToString());
            }
            

        }

        [HttpPost]
        [Route("~/api/RegistrarTipoGasto")]
        public async Task<IHttpActionResult> RegistrarTipoGasto([FromBody] TipoGastoViewModel tipoGastoVIaje)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var TipoGastoViaje = new TipoGastoViaje()
                    {
                        Nombre = tipoGastoVIaje.Nombre,
                        Diario = tipoGastoVIaje.Diario,
                        Empresa = tipoGastoVIaje.Empresa,
                        Activo = true
                    };
                    ctx.TipoGastoViaje.Add(TipoGastoViaje);
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest("Diario " + tipoGastoVIaje.Diario + " ya existe.");
            }
        }

        [HttpPost]
        [Route("~/api/RegistrarCategoriaGasto")]
        public async Task<IHttpActionResult> RegistrarCategoriaGastoViaje([FromBody] CategoriaGastoViewModel categoria)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var categoriaGasto = new CategoriaTipoGastoViaje()
                    {
                        IdTipoGastoViaje = categoria.idTipoGastoViaje,
                        Nombre = categoria.Nombre,
                        ProveedorPredefinido = categoria.ProveedorPredefinido,
                        CuentaContrapartida = categoria.CuentaContrapartida,
                        FacturaObligatoria = categoria.FacturaObligatoria,
                        Descripcion = categoria.Descripcion,
                        ImagenObligatoria = categoria.ImagenObligatoria,
                        Activo = true
                    };
                    
                        
                    
                    ctx.CategoriaTipoGastoViaje.Add(categoriaGasto);
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest("No se creo la categoria "+ e);
            }
        }


        [HttpPost]
        [Route("~/api/ActualizarEstadoGasto/{id}/{estado}/{mensaje}/{admin}/{mensajeAX}")]
        public async Task<IHttpActionResult> ActualizarEstadoGasto(int id, int estado, string mensaje,string admin, string mensajeAX)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var GastoViaje = await ctx.GastosViajeDetalle.FindAsync(id);
                    if (GastoViaje == null)
                    {
                        return BadRequest("No se encontro el Gasto");
                    }

                    GastoViaje.IdEstado = estado;
                    if(mensaje != "-")
                    {
                        GastoViaje.DescripcionAdmin = mensaje;
                    }

                    if(mensajeAX != "-")
                    {
                        GastoViaje.MensajeAX = mensajeAX;
                    }
                    if(admin != "-")
                    {
                        GastoViaje.Administrador = admin;
                    }
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/ActualizarEstadoTipo/{id}")]
        public async Task<IHttpActionResult> ActualizarEstadoTipo(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var Tipo = await ctx.TipoGastoViaje.FindAsync(id);
                    if (Tipo == null)
                    {
                        return BadRequest("No se encontro el Gasto");
                    }

                    Tipo.Activo = !Tipo.Activo;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/ActualizarNombreTipo/{id}/{Nombre}")]
        public async Task<IHttpActionResult> ActualizarNombreTipo(int id, string nombre)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var Tipo = await ctx.TipoGastoViaje.FindAsync(id);
                    if (Tipo == null)
                    {
                        return BadRequest("No se encontro el Gasto");
                    }

                    Tipo.Nombre = nombre;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/ActualizarEstadoCategoria/{id}")]
        public async Task<IHttpActionResult> ActualizarEstadoCategoria(int id)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var Categoria = await ctx.CategoriaTipoGastoViaje.FindAsync(id);
                    if (Categoria == null)
                    {
                        return BadRequest("No se encontro La categoria");
                    }

                    Categoria.Activo = !Categoria.Activo;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("~/api/ActualizarCategoria")]
        public async Task<IHttpActionResult> ActualizarNombreCategoria(CategoriaGastoViewModel categoria)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var Categoria = await ctx.CategoriaTipoGastoViaje.FindAsync(categoria.idCategoriaTipoGasto);
                    if (Categoria == null)
                    {
                        return BadRequest("No se encontro la Categoria");
                    }

                    Categoria.Nombre = categoria.Nombre;
                    Categoria.ProveedorPredefinido = categoria.ProveedorPredefinido;
                    Categoria.CuentaContrapartida = categoria.CuentaContrapartida;
                    Categoria.FacturaObligatoria = categoria.FacturaObligatoria;
                    Categoria.Descripcion = categoria.Descripcion;
                    Categoria.ImagenObligatoria = categoria.ImagenObligatoria;
                    var result = await ctx.SaveChangesAsync();
                    return Ok(result);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
    }
}
