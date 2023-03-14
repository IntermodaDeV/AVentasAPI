using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models.ViewModels;
using RestSharp;
using ExternalApiData.Enviroments;
using System.Data.Entity;
using AventasApi.Models;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;
namespace AventasApi.Controllers
{
    public class DeshabilitarProducto
    {
        public int Coleccion { get; set; }
        public string Pais { get; set; }
        public string Producto { get; set; }
    }

    public class ImagenColeccion
    {
        public string PACKAGEID { get; set; }
        public string IMAGE { get; set; }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [RoutePrefix("api/ColeccionesXLinea")]
    public class ColeccionesXLineaController : ApiController
    {
        readonly AVentasEntities context = new AVentasEntities();

        public ColeccionesXLineaController()
        {
            this.context.Database.CommandTimeout = 300;
            this.context.Configuration.LazyLoadingEnabled = false;
        }
        [HttpGet]
        [Route("~/api/colecciones/{codigoColeccion}/{empresa}/imagenesColeccion")]
        public async Task<IHttpActionResult> GetImagenes(string codigoColeccion, string empresa)
        {
            var Imagenes = new List<ImagenColeccion>();
            var client = new RestClient(Enviroment.CRMWebServiceURLApi);
            client.Authenticator = new RestSharp.Authenticators.NtlmAuthenticator();
            var request = new RestRequest($"paquetes/{empresa}/{codigoColeccion}/imagenespaquete", Method.GET);
            //client.Timeout = 6000;
            request.AddHeader("Accept", "application/json");
            IRestResponse respuesta = client.Execute(request);

            if (respuesta.IsSuccessful && respuesta.Content != "null")
            {
                Imagenes = JsonConvert.DeserializeObject<List<ImagenColeccion>>(respuesta.Content);
                using (AVentasEntities db = new AVentasEntities())
                {
                    var Colecciones = db.Colecciones.Where(c => c.CodigoColeccion == codigoColeccion).ToList();
                    var config = db.Configuraciones.FirstOrDefault(c => c.CodigoConfiguracion == "UrlImages");
                    if(Colecciones.Count() > 0 && Imagenes.Count() > 0)
                    {
                        Base64ToImage(Imagenes[0].IMAGE, Imagenes[0].PACKAGEID);
                        var url = config.Valor + Imagenes[0].PACKAGEID + ".jpg";
                        foreach (var coleccion in Colecciones)
                        {
                            coleccion.FotoPortada = url;
                        }
                        var result = await db.SaveChangesAsync();

                        return Ok(result);
                    }

                    return BadRequest("El paquete no tiene imagen.");
                }
            }
            return BadRequest(respuesta.ErrorMessage);
        }

        public Image Base64ToImage(string base64String, string Nombre)
        {
            var path = Properties.Settings.Default.PathImagenes;
            var filePath = $"{path}{Nombre}.jpg";
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                image.Save(filePath);
                return image;
            }
        }
        [HttpGet]
        [Route("{id}/{pais}")]
        public async Task<IHttpActionResult> GetcoleccionesXGrupoPrecio(string id,string pais)
        {
            var colecciones = await ObtenerColecciones(id, pais);
            return Ok(colecciones);
        }
        [HttpGet]
        public async Task<IHttpActionResult> Getcolecciones()
        {
            return Ok(ObtenerColecciones(null,"").Result);
        }
        private async Task<List<ColeccionViewModel>> ObtenerColecciones(string grupoPrecio,string pais)
        {

            bool filtarXGrupoPrecio = grupoPrecio != null;
            //var atributosXColeccionList = context.vw_AtributosxColeccion.ToList();
            string urlImagenes = context.Configuraciones.FirstOrDefault(conf => conf.CodigoConfiguracion == "UrlImages")?.Valor ?? "";
            List<ColeccionViewModel> colecciones = await context.Colecciones
                //.Include(co=>co.EdadesxColeccion)
                .Where(vw_coleccion => vw_coleccion.VentaInicio <= DateTime.Today && vw_coleccion.VentaFinal >= DateTime.Today && vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper()).OrderBy(vw_coleccion => vw_coleccion.VentaFinal).Select(vw_coleccion =>
                             new ColeccionViewModel
                             {
                                 IdColeccion = vw_coleccion.IdColeccion,
                                 CodigoColeccion = vw_coleccion.CodigoColeccion,
                                 Nombre = vw_coleccion.Nombre,
                                 ColeccionTipo = vw_coleccion.ColeccionTipo,
                                 EmpresaId = vw_coleccion.EmpresaId,
                                 FotoPortada = vw_coleccion.FotoPortada,
                                 DisenoInicio = vw_coleccion.DisenoInicio,
                                 DisenoFinal = vw_coleccion.DisenoFinal,
                                 EntregaInicio = vw_coleccion.EntregaInicio,
                                 EntregaFinal = vw_coleccion.EntregaFinal,
                                 Estatus = vw_coleccion.Estatus ?? 0,
                                 ProduccionInicio = vw_coleccion.ProduccionInicio,
                                 ProduccionFinal = vw_coleccion.ProduccionFinal,
                                 VentaInicio = vw_coleccion.VentaInicio,
                                 VentaFinal = vw_coleccion.VentaFinal,
                                 Lineas = vw_coleccion.LineasxColeccion.Select(colXLin => colXLin.IdLinea).ToList(),
                                 AtributosXColeccion = vw_coleccion.AtributosxColeccion.Select(atr => new AtributosViewModel
                                 {
                                     Descripcion = (atr.Descripcion2 == "BASE") ? atr.CodigoAtributo + " - " + atr.Descripcion1 : atr.Descripcion1,
                                     Tipo = atr.Descripcion2,
                                     IdLinea = atr.IdLinea
                                 }).ToList(),
                                 Edades = vw_coleccion.EdadesxColeccion
                                 .OrderBy(me => me.MaestroEdad.Orden).Select(me =>
                                                new EdadesViewModel
                                                {
                                                    IdEdad = me.IdEdad,
                                                    Edad = me.MaestroEdad.Edad,
                                                    Orden = me.MaestroEdad.Orden,
                                                    ProductosXEdad = context.ProductosxColeccion.Where(pxc => pxc.EmpresaId == pais.ToUpper() && pxc.IdColeccion == vw_coleccion.IdColeccion && pxc.IdEdad == me.IdEdad && me.IdLinea == pxc.IdLinea && pxc.VisibleParaVentas == true).Select(pxc => new ProductoXColeccionViewModel
                                                    {
                                                        ProductoId = pxc.CodigoProducto,
                                                        CodigoColeccion = vw_coleccion.CodigoColeccion,
                                                        CodigoProducto = pxc.IdProducto,
                                                        NombreProducto = pxc.NombreProducto,
                                                        GrupoImpuesto = (string.IsNullOrEmpty(pxc.GrupoImpuesto)) ? "GENERAL" : pxc.GrupoImpuesto.ToUpper(),
                                                        Precio = pxc.PreciosxProducto.Where(preEsp => true || !filtarXGrupoPrecio).Select(precio => new PrecioXProductoViewModel
                                                        {
                                                            GrupoPrecio = precio.GrupoPrecio,
                                                            IdMoneda = precio.IdMoneda,
                                                            Precio = precio.Hasta == new DateTime(1900, 1, 1) ? precio.Precio : 0
                                                        }).ToList(),
                                                        GrupoTalla = pxc.CodigoGrupoTalla,
                                                        Linea = new LineaViewModel
                                                        {
                                                            IdLinea = pxc.MaestroLinea.IdLinea,
                                                            Linea = pxc.MaestroLinea.Linea,
                                                        },
                                                        AtributosXProducto = pxc.AtributosxProducto.Select(atr => new AtributosViewModel
                                                        {
                                                            Descripcion = (atr.Descripcion2 == "BASE") ? atr.CodigoAtributo + " - " + atr.Descripcion1 : atr.Descripcion1,
                                                            Tipo = atr.Descripcion2,
                                                            IdLinea = pxc.IdLinea
                                                        }).ToList(),
                                                        ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null)
                                                             .Where(txp => (pxc.FotografiasXProducto.Where(fxp => fxp.CodigoColor == "").Count() > 0 && txp.CodigoColor == "") || (pxc.FotografiasXProducto.FirstOrDefault() != null
                                                             && txp.CodigoColor == (pxc.FotografiasXProducto.Where(c => c.CodigoColor != null).Count() > 0 ? (pxc.ColoresxProducto.Where(c => c.Disponible == true).Count() > 0 ? pxc.ColoresxProducto.FirstOrDefault().CodigoColor : pxc.FotografiasXProducto.FirstOrDefault().CodigoColor) : pxc.FotografiasXProducto.FirstOrDefault().CodigoColor)))
                                                             .OrderByDescending(foto => foto.Principal)
                                                             .Select(foto => new FotografiasXProductoViewModel
                                                             {
                                                                 IdFotografia = foto.IdFotografia,
                                                                 FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                                                 CodigoColor = foto.CodigoColor,
                                                                 Principal = foto.Principal ?? false
                                                             }).ToList(),
                                                        ListaTalla = context.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto).Select(txp => txp.TallasXGrupo).Where(txp => false || (vw_coleccion.ColeccionTipo == "F") || txp.DistribucionxTalla.Count > 0 || pxc.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) >= 0)
                                                        .Select(txp => new TallaViewModel
                                                        {
                                                            Talla = txp.CodigoTalla,
                                                            GrupoTallaId = txp.CodigoGrupoTalla,
                                                            Orden = txp.Orden ?? 0,
                                                            Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                                            {
                                                                IdDistribucion = dis.IdDistribucion,
                                                                IdTallaxGrupo = dis.IdTallaxGrupo,
                                                                NombreDistribucion = dis.NombreDistribucion,
                                                                NombreTalla = dis.NombreTalla,
                                                                Cantidad = dis.Cantidad,
                                                                Orden = dis.Orden,
                                                            }).OrderBy(or => or.Orden).ToList(),

                                                        }).OrderBy(txp => txp.Orden).ToList(),
                                                        ListaColores = pxc.ColoresxProducto.Where(cpp => (vw_coleccion.ColeccionTipo == "F") || cpp.Colores.FisicoDisponible.Where(f => f.IdProducto == pxc.IdProducto).Sum(f => f.Disponible) > 0).OrderBy(cpp => cpp.Colores.Color).Select(cpp => new ColorViewModel
                                                        {
                                                            CodigoColor = cpp.Colores.CodigoColor,
                                                            NombreColor = cpp.Colores.Color,
                                                            Color = cpp.Colores.Rgb,
                                                            ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null && txp.CodigoColor == cpp.Colores.CodigoColor).Select(foto => new FotografiasXProductoViewModel
                                                            {
                                                                IdFotografia = foto.IdFotografia,
                                                                FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                                                CodigoColor = foto.CodigoColor,
                                                                Principal = foto.Principal ?? false
                                                            }).ToList(),

                                                        }).ToList(),
                                                        fisicaDisponible = pxc.FisicoDisponible
                                                           /*.Where(f => (vw_coleccion.ColeccionTipo == "F") || f.Disponible >= 0)*/
                                                           .Select(f => new FisicoDisponibleViewModel
                                                           {
                                                               CodigoColor = f.CodigoColor,
                                                               IdTalla = f.CodigoTalla,
                                                               Cantidad = f.Disponible,
                                                               MinStock = f.MinStock,
                                                               PreciosEspecificos = f.PrecioEspecifico.Where(preEsp => filtarXGrupoPrecio).Where(preEsp => preEsp.GrupoPrecio == grupoPrecio).Select(preEsp => new PrecioEspecificoViewModel
                                                               {
                                                                   IdPrecioEspecifico = preEsp.IdPrecioEspecifico,
                                                                   IdMoneda = preEsp.IdMoneda,
                                                                   IdProducto = preEsp.IdProducto,
                                                                   GrupoPrecio = preEsp.GrupoPrecio,
                                                                   IdFisicoDisponible = preEsp.IdFisicoDisponible,
                                                                   Precio = preEsp.Hasta == new DateTime(1900, 1, 1) ? preEsp.Precio : 0,
                                                               }).ToList(),
                                                           }).ToList()
                                                    }).ToList()
                                                }
                                 ).ToList()
                             }).AsNoTracking().ToListAsync();

            return colecciones;
        }

        [HttpGet]
        [Route("~/api/colecciones/bodega")]
        public async Task<IHttpActionResult> ObtenerColeccionesBodega()
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    var colecciones = await ctx.Colecciones.Where(x => x.ColeccionTipo != "F" && x.ColeccionTipo != "N/A" && x.ColeccionTipo != "W").Select(x => new { Codigo = x.CodigoColeccion, Empresa = x.EmpresaId }).ToListAsync();
                    return Ok(colecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/colecciones/{linea}/{pais}/{almacen}/{sitio}")]
        public async Task<IHttpActionResult> GetColeccionesPorLinea(string linea, string pais, string almacen, string sitio)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var SitioPrincipal = ctx.Configuraciones.Where(s => s.CodigoConfiguracion == "SitioPrincipal").Select(s => s.Valor).FirstOrDefault();
                    var BodegaPrincipal = ctx.Configuraciones.Where(s => s.CodigoConfiguracion == "BodegaPrincipal").Select(s => s.Valor).FirstOrDefault();

                    if (SitioPrincipal != sitio || BodegaPrincipal != almacen)
                    {
                        var Paquete = await ctx.PaqueteBodegaEspecifico.Where(pb => pb.Sitio == sitio && pb.Almacen == almacen).Select(c => c.ColeccionId).ToListAsync();

                        List<ColeccionViewModel> ColeccioneBE = await ctx.Colecciones
                        .Where(vw_coleccion => vw_coleccion.VentaInicio <= DateTime.Today
                               && vw_coleccion.VentaFinal >= DateTime.Today
                               && vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper()
                               && Paquete.Contains(vw_coleccion.IdColeccion)
                               && vw_coleccion.LineasxColeccion.Select(x => x.IdLinea).Contains(linea))
                        .OrderBy(vw_coleccion => vw_coleccion.VentaFinal).Select(vw_coleccion => new ColeccionViewModel
                        {
                            IdColeccion = vw_coleccion.IdColeccion,
                            CodigoColeccion = vw_coleccion.CodigoColeccion,
                            Nombre = vw_coleccion.Nombre,
                            ColeccionTipo = vw_coleccion.ColeccionTipo,
                            EmpresaId = vw_coleccion.EmpresaId,
                            FotoPortada = vw_coleccion.FotoPortada,
                            DisenoInicio = vw_coleccion.DisenoInicio,
                            DisenoFinal = vw_coleccion.DisenoFinal,
                            EntregaInicio = vw_coleccion.EntregaInicio,
                            EntregaFinal = vw_coleccion.EntregaFinal,
                            Estatus = vw_coleccion.Estatus,
                            ProduccionInicio = vw_coleccion.ProduccionInicio,
                            ProduccionFinal = vw_coleccion.ProduccionFinal,
                            VentaInicio = vw_coleccion.VentaInicio,
                            VentaFinal = vw_coleccion.VentaFinal,
                            Lineas = vw_coleccion.LineasxColeccion.Select(colXLin => colXLin.IdLinea).ToList(),
                            AtributosXColeccion = vw_coleccion.AtributosxColeccion.Select(atr => new AtributosViewModel
                            {
                                Descripcion = (atr.Descripcion2 == "BASE") ? atr.CodigoAtributo + " - " + atr.Descripcion1 : atr.Descripcion1,
                                Tipo = atr.Descripcion2,
                                IdLinea = atr.IdLinea
                            }).ToList()
                        }).ToListAsync();

                        return Ok(ColeccioneBE);
                    }

                    var paquetesBodegaEspecifico = await ctx.PaqueteBodegaEspecifico.Select(x => x.ColeccionId).ToListAsync();
                    List<ColeccionViewModel> colecciones = await ctx.Colecciones
                        .Where(vw_coleccion => vw_coleccion.VentaInicio <= DateTime.Today
                               && !paquetesBodegaEspecifico.Contains(vw_coleccion.IdColeccion)
                               && vw_coleccion.VentaFinal >= DateTime.Today
                               && vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper()
                               && vw_coleccion.LineasxColeccion.Select(x => x.IdLinea).Contains(linea))
                        .OrderBy(vw_coleccion => vw_coleccion.VentaFinal).Select(vw_coleccion => new ColeccionViewModel
                        {
                            IdColeccion = vw_coleccion.IdColeccion,
                            CodigoColeccion = vw_coleccion.CodigoColeccion,
                            Nombre = vw_coleccion.Nombre,
                            ColeccionTipo = vw_coleccion.ColeccionTipo,
                            EmpresaId = vw_coleccion.EmpresaId,
                            FotoPortada = vw_coleccion.FotoPortada,
                            DisenoInicio = vw_coleccion.DisenoInicio,
                            DisenoFinal = vw_coleccion.DisenoFinal,
                            EntregaInicio = vw_coleccion.EntregaInicio,
                            EntregaFinal = vw_coleccion.EntregaFinal,
                            Estatus = vw_coleccion.Estatus,
                            ProduccionInicio = vw_coleccion.ProduccionInicio,
                            ProduccionFinal = vw_coleccion.ProduccionFinal,
                            VentaInicio = vw_coleccion.VentaInicio,
                            VentaFinal = vw_coleccion.VentaFinal,
                            Lineas = vw_coleccion.LineasxColeccion.Select(colXLin => colXLin.IdLinea).ToList(),
                            AtributosXColeccion = vw_coleccion.AtributosxColeccion.Select(atr => new AtributosViewModel
                            {
                                Descripcion = (atr.Descripcion2 == "BASE") ? atr.CodigoAtributo + " - " + atr.Descripcion1 : atr.Descripcion1,
                                Tipo = atr.Descripcion2,
                                IdLinea = atr.IdLinea
                            }).ToList()
                        }).ToListAsync();

                    return Ok(colecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/colecciones/productos/{coleccion}/{grupoprecio}/{pais}")]
        public async Task<IHttpActionResult> GetProductosPorColeccion(string coleccion, string grupoprecio, string pais)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    string urlImagenes = ctx.Configuraciones.FirstOrDefault(conf => conf.CodigoConfiguracion == "UrlImages")?.Valor ?? "";
                    bool filtarXGrupoPrecio = grupoprecio != null;
                    List<ColeccionViewModel> colecciones = await ctx.Colecciones
                        .Where(vw_coleccion => vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper() && vw_coleccion.CodigoColeccion == coleccion.ToUpper())
                        .Select(vw_coleccion =>
                             new ColeccionViewModel
                             {
                                 Edades = vw_coleccion.EdadesxColeccion
                               .OrderBy(me => me.MaestroEdad.Orden).Select(me => new EdadesViewModel
                               {
                                   IdEdad = me.IdEdad,
                                   Edad = me.MaestroEdad.Edad,
                                   Orden = me.MaestroEdad.Orden,
                                   ProductosXEdad = ctx.ProductosxColeccion.Where(pxc => pxc.EmpresaId == pais.ToUpper() && pxc.IdColeccion == vw_coleccion.IdColeccion && pxc.IdEdad == me.IdEdad && pxc.IdLinea == me.IdLinea && pxc.VisibleParaVentas == true).Select(pxc => new ProductoXColeccionViewModel
                                   {
                                       ProductoId = pxc.CodigoProducto,
                                       idColeccion = pxc.IdColeccion,
                                       CodigoColeccion = vw_coleccion.CodigoColeccion,
                                       CantidadMinima = pxc.CantidadMinima == null ? 0 : pxc.CantidadMinima,
                                       CodigoProducto = pxc.IdProducto,
                                       NombreProducto = pxc.NombreProducto,
                                       StockVisible = pxc.StockVisible,
                                       GrupoImpuesto = (string.IsNullOrEmpty(pxc.GrupoImpuesto)) ? "GENERAL" : pxc.GrupoImpuesto.ToUpper(),
                                       InOut = pxc.InOut,
                                       Deshabilitado = pxc.Deshabilitado,
                                       Prioridad = pxc.Prioridad,
                                       Precio = pxc.PreciosxProducto.Where(preEsp =>/* true || */preEsp.GrupoPrecio == grupoprecio).Select(precio => new PrecioXProductoViewModel
                                       {
                                           GrupoPrecio = precio.GrupoPrecio,
                                           IdMoneda = precio.IdMoneda,
                                           Precio = precio.Hasta == new DateTime(1900, 1, 1) ? precio.Precio : 0
                                       }).ToList(),
                                       GrupoTalla = pxc.CodigoGrupoTalla,
                                       Linea = new LineaViewModel
                                       {
                                           IdLinea = pxc.MaestroLinea.IdLinea,
                                           Linea = pxc.MaestroLinea.Linea,
                                       },
                                       AtributosXProducto = pxc.AtributosxProducto.Select(atr => new AtributosViewModel
                                       {
                                           Descripcion = (atr.Descripcion2 == "BASE") ? atr.CodigoAtributo + " - " + atr.Descripcion1 : atr.Descripcion1,
                                           Tipo = atr.Descripcion2,
                                           IdLinea = pxc.IdLinea
                                       }).ToList(),
                                       ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null)
                                              .Where(txp => (pxc.FotografiasXProducto.Where(fxp => fxp.CodigoColor == "").Count() > 0 && txp.CodigoColor == "") || (pxc.FotografiasXProducto.FirstOrDefault() != null && txp.CodigoColor == pxc.FotografiasXProducto.FirstOrDefault().CodigoColor))
                                              .OrderByDescending(foto => foto.Principal)
                                              .Select(foto => new FotografiasXProductoViewModel
                                              {
                                                  IdFotografia = foto.IdFotografia,
                                                  FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                                  NombreFotografia = foto.FotografiaProducto,
                                                  CodigoColor = foto.CodigoColor,
                                                  Principal = foto.Principal ?? false
                                              }).ToList(),
                                       ListaTalla = ctx.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto && txp.IdTallaxGrupo != null).Select(txp => txp.TallasXGrupo).Where(txp => false || (vw_coleccion.ColeccionTipo == "F") || txp.DistribucionxTalla.Count > 0 || pxc.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) >= 0)
                                           .Select(txp => new TallaViewModel
                                           {
                                               Talla = txp.CodigoTalla.ToUpper(),
                                               GrupoTallaId = txp.CodigoGrupoTalla,
                                               Orden = txp.Orden ?? 0,
                                               Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                               {
                                                   IdDistribucion = dis.IdDistribucion,
                                                   IdTallaxGrupo = dis.IdTallaxGrupo,
                                                   NombreDistribucion = dis.NombreDistribucion,
                                                   NombreTalla = dis.NombreTalla,
                                                   Cantidad = dis.Cantidad,
                                                   Orden = dis.Orden,
                                               }).OrderBy(or => or.Orden).ToList(),

                                           }).OrderBy(txp => txp.Orden).ToList(),
                                       ListaColores = pxc.ColoresxProducto.Where(cpp => (vw_coleccion.ColeccionTipo == "F") || cpp.Colores.FisicoDisponible.Where(f => f.IdProducto == pxc.IdProducto).Sum(f => f.Disponible) > 0).OrderBy(cpp => cpp.Colores.Color).Select(cpp => new ColorViewModel
                                       {
                                           CodigoColor = cpp.Colores.CodigoColor,
                                           NombreColor = cpp.Colores.Color,
                                           Color = cpp.Colores.Rgb,
                                           Prioridad = cpp.Prioridad,
                                           Deshabilitado = cpp.Deshabilitado,
                                           IdColorxProducto = cpp.IdColorxProducto,
                                           ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null && txp.CodigoColor == cpp.Colores.CodigoColor).Select(foto => new FotografiasXProductoViewModel
                                           {
                                               IdFotografia = foto.IdFotografia,
                                               FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                               NombreFotografia = foto.FotografiaProducto,
                                               CodigoColor = foto.CodigoColor,
                                               Principal = foto.Principal ?? false
                                           }).ToList(),

                                       }).ToList(),
                                       ListaColoresSinStock = pxc.ColoresxProducto.Where(cpp => (vw_coleccion.ColeccionTipo == "F") || cpp.Colores.FisicoDisponible.Where(f => f.IdProducto == pxc.IdProducto).Sum(f => f.Disponible) == 0).OrderBy(cpp => cpp.Colores.Color).Select(cpp => new ColorSinStock
                                       {
                                           CodigoColor = cpp.Colores.CodigoColor,
                                           NombreColor = cpp.Colores.Color,
                                           Color = cpp.Colores.Rgb,
                                       }).ToList(),
                                       fisicaDisponible = pxc.FisicoDisponible
                                              /*.Where(f => (vw_coleccion.ColeccionTipo == "F") || f.Disponible >= 0)*/
                                              .Select(f => new FisicoDisponibleViewModel
                                              {
                                                  CodigoColor = f.CodigoColor,
                                                  IdTalla = f.CodigoTalla.ToUpper(),
                                                  Cantidad = f.Disponible < 0 ? 0 : f.Disponible,
                                                  MinStock = f.MinStock,
                                                  PreciosEspecificos = f.PrecioEspecifico.Where(preEsp => filtarXGrupoPrecio).Where(preEsp => preEsp.GrupoPrecio == grupoprecio).Select(preEsp => new PrecioEspecificoViewModel
                                                  {
                                                      IdPrecioEspecifico = preEsp.IdPrecioEspecifico,
                                                      IdMoneda = preEsp.IdMoneda,
                                                      IdProducto = preEsp.IdProducto,
                                                      GrupoPrecio = preEsp.GrupoPrecio,
                                                      IdFisicoDisponible = preEsp.IdFisicoDisponible,
                                                      Precio = preEsp.Hasta == new DateTime(1900, 1, 1) ? preEsp.Precio : pxc.PreciosxProducto.FirstOrDefault(pre => pre.GrupoPrecio == grupoprecio && pre.IdProducto == preEsp.IdProducto).Precio,
                                                  }).ToList(),
                                              }).ToList()
                                   }).ToList()
                               }
                               ).ToList()

                             }).ToListAsync();

                    return Ok(colecciones[0].Edades);

                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/producto/{pais}/{grupoPrecio}/{producto}/{color}")]
        public async Task<IHttpActionResult> GetProducto(string pais, string grupoPrecio, string producto, string color)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var prod = ctx.ProductosxColeccion.Where(pxc => pxc.EmpresaId == pais.ToUpper() && pxc.CodigoProducto == producto).Select(pxc => new ProductoXColeccionViewModel
                    {
                        ProductoId = pxc.CodigoProducto,
                        idColeccion = pxc.IdColeccion,
                        CantidadMinima = pxc.CantidadMinima == null ? 0 : pxc.CantidadMinima,
                        CodigoProducto = pxc.IdProducto,
                        NombreProducto = pxc.NombreProducto,
                        StockVisible = pxc.StockVisible,
                        InOut = pxc.InOut,
                        Deshabilitado = pxc.Deshabilitado,
                        Prioridad = pxc.Prioridad,
                        GrupoImpuesto = (string.IsNullOrEmpty(pxc.GrupoImpuesto)) ? "GENERAL" : pxc.GrupoImpuesto.ToUpper(),
                        Precio = pxc.PreciosxProducto.Where(preEsp => preEsp.GrupoPrecio == grupoPrecio).Select(precio => new PrecioXProductoViewModel
                        {
                            GrupoPrecio = precio.GrupoPrecio,
                            IdMoneda = precio.IdMoneda,
                            Precio = precio.Hasta == new DateTime(1900, 1, 1) ? precio.Precio : 0
                        }).ToList(),
                        GrupoTalla = pxc.CodigoGrupoTalla,
                        Linea = new LineaViewModel
                        {
                            IdLinea = pxc.MaestroLinea.IdLinea,
                            Linea = pxc.MaestroLinea.Linea,
                        },
                        ListaTalla = ctx.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto && txp.IdTallaxGrupo != null).Select(txp => txp.TallasXGrupo)
                                             .Select(txp => new TallaViewModel
                                             {
                                                 Talla = txp.CodigoTalla.ToUpper(),
                                                 GrupoTallaId = txp.CodigoGrupoTalla,
                                                 Orden = txp.Orden ?? 0,
                                                 Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                                 {
                                                     IdDistribucion = dis.IdDistribucion,
                                                     IdTallaxGrupo = dis.IdTallaxGrupo,
                                                     NombreDistribucion = dis.NombreDistribucion,
                                                     NombreTalla = dis.NombreTalla,
                                                     Cantidad = dis.Cantidad,
                                                     Orden = dis.Orden,
                                                 }).OrderBy(or => or.Orden).ToList(),

                                             }).OrderBy(txp => txp.Orden).ToList(),
                        ListaColores = ctx.Colores.Where(x => x.CodigoColor == color).Select(cpp => new ColorViewModel
                        {
                            CodigoColor = cpp.CodigoColor,
                            NombreColor = cpp.Color,
                            Color = cpp.Rgb
                        }).ToList(),
                        fisicaDisponible = pxc.FisicoDisponible
                                              .Select(f => new FisicoDisponibleViewModel
                                              {
                                                  CodigoColor = f.CodigoColor,
                                                  IdTalla = f.CodigoTalla.ToUpper(),
                                                  Cantidad = f.Disponible < 0 ? 0 : f.Disponible,
                                                  MinStock = f.MinStock,
                                                  PreciosEspecificos = f.PrecioEspecifico.Where(preEsp => preEsp.GrupoPrecio == grupoPrecio).Select(preEsp => new PrecioEspecificoViewModel
                                                  {
                                                      IdPrecioEspecifico = preEsp.IdPrecioEspecifico,
                                                      IdMoneda = preEsp.IdMoneda,
                                                      IdProducto = preEsp.IdProducto,
                                                      GrupoPrecio = preEsp.GrupoPrecio,
                                                      IdFisicoDisponible = preEsp.IdFisicoDisponible,
                                                      Precio = preEsp.Hasta == new DateTime(1900, 1, 1) ? preEsp.Precio : pxc.PreciosxProducto.FirstOrDefault(pre => pre.GrupoPrecio == grupoPrecio && pre.IdProducto == preEsp.IdProducto).Precio,
                                                  }).ToList(),
                                              }).ToList()
                    }).OrderByDescending(x=> x.ListaTalla.Count()).ToList();
                    return Ok(prod.First());
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/colecciones/productos/{coleccion}/{grupoprecio}/{pais}/{sitio}/{almacen}")]
        public async Task<IHttpActionResult> GetProductosPorColeccionBodega(string coleccion, string grupoprecio, string pais, string sitio, string almacen)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    string urlImagenes = ctx.Configuraciones.FirstOrDefault(conf => conf.CodigoConfiguracion == "UrlImages")?.Valor ?? "";
                    bool filtarXGrupoPrecio = grupoprecio != null;
                    List<ColeccionViewModel> colecciones = await ctx.Colecciones
                        .Where(vw_coleccion => vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper() && vw_coleccion.CodigoColeccion == coleccion.ToUpper())
                        .Select(vw_coleccion =>
                             new ColeccionViewModel
                             {
                                 Edades = vw_coleccion.EdadesxColeccion
                               .OrderBy(me => me.MaestroEdad.Orden).Select(me => new EdadesViewModel
                               {
                                   IdEdad = me.IdEdad,
                                   Edad = me.MaestroEdad.Edad,
                                   Orden = me.MaestroEdad.Orden,
                                   ProductosXEdad = ctx.ProductosxColeccion.Where(pxc => pxc.EmpresaId == pais.ToUpper() && pxc.IdColeccion == vw_coleccion.IdColeccion && pxc.IdEdad == me.IdEdad && pxc.IdLinea == me.IdLinea && pxc.VisibleParaVentas == true).Select(pxc => new ProductoXColeccionViewModel
                                   {
                                       ProductoId = pxc.CodigoProducto,
                                       CodigoColeccion = vw_coleccion.CodigoColeccion,
                                       CantidadMinima = pxc.CantidadMinima == null ? 0 : pxc.CantidadMinima,
                                       CodigoProducto = pxc.IdProducto,
                                       NombreProducto = pxc.NombreProducto,
                                       StockVisible = pxc.StockVisible,
                                       GrupoImpuesto = (string.IsNullOrEmpty(pxc.GrupoImpuesto)) ? "GENERAL" : pxc.GrupoImpuesto.ToUpper(),
                                       InOut = pxc.InOut,
                                       Deshabilitado = pxc.Deshabilitado,
                                       Prioridad = pxc.Prioridad,
                                       Precio = pxc.PreciosxProducto.Where(preEsp =>/* true || */preEsp.GrupoPrecio == grupoprecio).Select(precio => new PrecioXProductoViewModel
                                       {
                                           GrupoPrecio = precio.GrupoPrecio,
                                           IdMoneda = precio.IdMoneda,
                                           Precio = precio.Hasta == new DateTime(1900, 1, 1) ? precio.Precio : 0
                                       }).ToList(),
                                       GrupoTalla = pxc.CodigoGrupoTalla,
                                       Linea = new LineaViewModel
                                       {
                                           IdLinea = pxc.MaestroLinea.IdLinea,
                                           Linea = pxc.MaestroLinea.Linea,
                                       },
                                       AtributosXProducto = pxc.AtributosxProducto.Select(atr => new AtributosViewModel
                                       {
                                           Descripcion = (atr.Descripcion2 == "BASE") ? atr.CodigoAtributo + " - " + atr.Descripcion1 : atr.Descripcion1,
                                           Tipo = atr.Descripcion2,
                                           IdLinea = pxc.IdLinea
                                       }).ToList(),
                                       ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null)
                                              .Where(txp => (pxc.FotografiasXProducto.Where(fxp => fxp.CodigoColor == "").Count() > 0 && txp.CodigoColor == "") || (pxc.FotografiasXProducto.FirstOrDefault() != null && txp.CodigoColor == pxc.FotografiasXProducto.FirstOrDefault().CodigoColor))
                                              .OrderByDescending(foto => foto.Principal)
                                              .Select(foto => new FotografiasXProductoViewModel
                                              {
                                                  IdFotografia = foto.IdFotografia,
                                                  FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                                  NombreFotografia = foto.FotografiaProducto,
                                                  CodigoColor = foto.CodigoColor,
                                                  Principal = foto.Principal ?? false
                                              }).ToList(),
                                       ListaTalla = ctx.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto && txp.IdTallaxGrupo != null).Select(txp => txp.TallasXGrupo).Where(txp => false || (vw_coleccion.ColeccionTipo == "F") || txp.DistribucionxTalla.Count > 0 || pxc.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) >= 0)
                                           .Select(txp => new TallaViewModel
                                           {
                                               Talla = txp.CodigoTalla.ToUpper(),
                                               GrupoTallaId = txp.CodigoGrupoTalla,
                                               Orden = txp.Orden ?? 0,
                                               Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                               {
                                                   IdDistribucion = dis.IdDistribucion,
                                                   IdTallaxGrupo = dis.IdTallaxGrupo,
                                                   NombreDistribucion = dis.NombreDistribucion,
                                                   NombreTalla = dis.NombreTalla,
                                                   Cantidad = dis.Cantidad,
                                                   Orden = dis.Orden,
                                               }).OrderBy(or => or.Orden).ToList(),

                                           }).OrderBy(txp => txp.Orden).ToList(),
                                       ListaColores = pxc.ColoresxProducto.Where(cpp => (vw_coleccion.ColeccionTipo == "F") || cpp.Colores.FisicoDisponible.Where(f => f.IdProducto == pxc.IdProducto).Sum(f => f.Disponible) > 0).OrderBy(cpp => cpp.Colores.Color).Select(cpp => new ColorViewModel
                                       {
                                           CodigoColor = cpp.Colores.CodigoColor,
                                           NombreColor = cpp.Colores.Color,
                                           Color = cpp.Colores.Rgb,
                                           Prioridad = cpp.Prioridad,
                                           Deshabilitado = cpp.Deshabilitado,
                                           IdColorxProducto = cpp.IdColorxProducto,
                                           ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null && txp.CodigoColor == cpp.Colores.CodigoColor).Select(foto => new FotografiasXProductoViewModel
                                           {
                                               IdFotografia = foto.IdFotografia,
                                               FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                               NombreFotografia = foto.FotografiaProducto,
                                               CodigoColor = foto.CodigoColor,
                                               Principal = foto.Principal ?? false
                                           }).ToList(),

                                       }).ToList(),
                                       ListaColoresSinStock = pxc.ColoresxProducto.Where(cpp => (vw_coleccion.ColeccionTipo == "F") || cpp.Colores.FisicoDisponible.Where(f => f.IdProducto == pxc.IdProducto).Sum(f => f.Disponible) == 0).OrderBy(cpp => cpp.Colores.Color).Select(cpp => new ColorSinStock
                                       {
                                           CodigoColor = cpp.Colores.CodigoColor,
                                           NombreColor = cpp.Colores.Color,
                                           Color = cpp.Colores.Rgb,
                                       }).ToList(),
                                       fisicaDisponible = pxc.FisicoDisponible
                                              .Where(f => f.Sitio == sitio && f.Almacen == almacen)
                                              .Select(f => new FisicoDisponibleViewModel
                                              {
                                                  CodigoColor = f.CodigoColor,
                                                  IdTalla = f.CodigoTalla.ToUpper(),
                                                  Cantidad = f.Disponible < 0 ? 0 : f.Disponible,
                                                  MinStock = f.MinStock,
                                                  PreciosEspecificos = f.PrecioEspecifico.Where(preEsp => filtarXGrupoPrecio).Where(preEsp => preEsp.GrupoPrecio == grupoprecio).Select(preEsp => new PrecioEspecificoViewModel
                                                  {
                                                      IdPrecioEspecifico = preEsp.IdPrecioEspecifico,
                                                      IdMoneda = preEsp.IdMoneda,
                                                      IdProducto = preEsp.IdProducto,
                                                      GrupoPrecio = preEsp.GrupoPrecio,
                                                      IdFisicoDisponible = preEsp.IdFisicoDisponible,
                                                      Precio = preEsp.Hasta == new DateTime(1900, 1, 1) ? preEsp.Precio : pxc.PreciosxProducto.FirstOrDefault(pre => pre.GrupoPrecio == grupoprecio && pre.IdProducto == preEsp.IdProducto).Precio,
                                                  }).ToList(),
                                              }).ToList()
                                   }).ToList()
                               }
                               ).ToList()

                             }).ToListAsync();

                    return Ok(colecciones[0].Edades);

                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/colecciones/listaprecios")]
        public async Task<IHttpActionResult> ObtenerColecciones([FromUri] ListaPrecio grupo)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    bool filtarXGrupoPrecio = true;
                    string urlImagenes = ctx.Configuraciones.FirstOrDefault(conf => conf.CodigoConfiguracion == "UrlImages")?.Valor ?? "";
                    List<ColeccionViewModel> listaColecciones = new List<ColeccionViewModel>();
                    List<string> ListaGrupoPrecios = new List<string>();
                    var paquetesBodegaEspecifico = await ctx.PaqueteBodegaEspecifico.Select(x => x.ColeccionId).ToListAsync();

                    foreach (var pais in grupo.Paises)
                    {
                        ListaGrupoPrecios = ctx.MaestroGrupoPrecio.Where(x => grupo.ListaPrecios.Contains(x.GrupoPrecio) && x.EmpresaId == pais).Select(x => x.GrupoPrecio).ToList();
                        List<ColeccionViewModel> colecciones = await ctx.Colecciones
                            .Where(vw_coleccion =>vw_coleccion.Estatus == 1 && vw_coleccion.VentaInicio <= DateTime.Today && vw_coleccion.VentaFinal >= DateTime.Today
                                   && vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper() && !paquetesBodegaEspecifico.Contains(vw_coleccion.IdColeccion)).OrderBy(vw_coleccion => vw_coleccion.VentaFinal).Select(vw_coleccion =>
                                         new ColeccionViewModel
                                         {
                                             GrupoPrecio = ListaGrupoPrecios.FirstOrDefault(),
                                             IdColeccion = vw_coleccion.IdColeccion,
                                             CodigoColeccion = vw_coleccion.CodigoColeccion,
                                             Nombre = vw_coleccion.Nombre,
                                             ColeccionTipo = vw_coleccion.ColeccionTipo,
                                             EmpresaId = vw_coleccion.EmpresaId,
                                             FotoPortada = vw_coleccion.FotoPortada,
                                             DisenoInicio = vw_coleccion.DisenoInicio,
                                             DisenoFinal = vw_coleccion.DisenoFinal,
                                             EntregaInicio = vw_coleccion.EntregaInicio,
                                             EntregaFinal = vw_coleccion.EntregaFinal,
                                             Estatus = vw_coleccion.Estatus,
                                             ProduccionInicio = vw_coleccion.ProduccionInicio,
                                             ProduccionFinal = vw_coleccion.ProduccionFinal,
                                             VentaInicio = vw_coleccion.VentaInicio,
                                             VentaFinal = vw_coleccion.VentaFinal,
                                             Lineas = vw_coleccion.LineasxColeccion.Select(colXLin => colXLin.IdLinea).ToList(),
                                             AtributosXColeccion = vw_coleccion.AtributosxColeccion.Select(atr => new AtributosViewModel
                                             {
                                                 Descripcion = (atr.Descripcion2 == "BASE") ? atr.CodigoAtributo + " - " + atr.Descripcion1 : atr.Descripcion1,
                                                 Tipo = atr.Descripcion2,
                                                 IdLinea = atr.IdLinea
                                             }).ToList(),
                                             Edades = vw_coleccion.EdadesxColeccion
                                             .OrderBy(me => me.MaestroEdad.Orden).Select(me =>
                                                            new EdadesViewModel
                                                            {
                                                                IdEdad = me.IdEdad,
                                                                Edad = me.MaestroEdad.Edad,
                                                                Orden = me.MaestroEdad.Orden,
                                                                ProductosXEdad = ctx.ProductosxColeccion.Where(pxc => pxc.EmpresaId == pais.ToUpper() && pxc.IdColeccion == vw_coleccion.IdColeccion && pxc.IdEdad == me.IdEdad && me.IdLinea == pxc.IdLinea && pxc.VisibleParaVentas == true).Select(pxc => new ProductoXColeccionViewModel
                                                                {
                                                                    ProductoId = pxc.CodigoProducto,
                                                                    CantidadMinima = pxc.CantidadMinima == null ? 0 : pxc.CantidadMinima,
                                                                    CodigoColeccion = vw_coleccion.CodigoColeccion,
                                                                    CodigoProducto = pxc.IdProducto,
                                                                    NombreProducto = pxc.NombreProducto,
                                                                    StockVisible = pxc.StockVisible,
                                                                    GrupoImpuesto = (string.IsNullOrEmpty(pxc.GrupoImpuesto)) ? "GENERAL" : pxc.GrupoImpuesto.ToUpper(),
                                                                    Precio = pxc.PreciosxProducto.Where(preEsp => true || !filtarXGrupoPrecio).Select(precio => new PrecioXProductoViewModel
                                                                    {
                                                                        GrupoPrecio = precio.GrupoPrecio,
                                                                        IdMoneda = precio.IdMoneda,
                                                                        Precio = precio.Hasta == new DateTime(1900, 1, 1) ? precio.Precio : 0
                                                                    }).ToList(),
                                                                    GrupoTalla = pxc.CodigoGrupoTalla,
                                                                    Linea = new LineaViewModel
                                                                    {
                                                                        IdLinea = pxc.MaestroLinea.IdLinea,
                                                                        Linea = pxc.MaestroLinea.Linea,
                                                                    },
                                                                    AtributosXProducto = pxc.AtributosxProducto.Select(atr => new AtributosViewModel
                                                                    {
                                                                        Descripcion = (atr.Descripcion2 == "BASE") ? atr.CodigoAtributo + " - " + atr.Descripcion1 : atr.Descripcion1,
                                                                        Tipo = atr.Descripcion2,
                                                                        IdLinea = pxc.IdLinea
                                                                    }).ToList(),
                                                                    ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null)
                                                                       .Where(txp => (pxc.FotografiasXProducto.Where(fxp => fxp.CodigoColor == "").Count() > 0 && txp.CodigoColor == "") || (pxc.FotografiasXProducto.FirstOrDefault() != null && txp.CodigoColor == pxc.FotografiasXProducto.FirstOrDefault().CodigoColor))
                                                                       .OrderByDescending(foto => foto.Principal)
                                                                       .Select(foto => new FotografiasXProductoViewModel
                                                                       {
                                                                           IdFotografia = foto.IdFotografia,
                                                                           FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                                                           CodigoColor = foto.CodigoColor,
                                                                           Principal = foto.Principal ?? false
                                                                       }).ToList(),
                                                                    ListaTalla = ctx.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto && txp.IdTallaxGrupo != null).Select(txp => txp.TallasXGrupo).Where(txp => false || (vw_coleccion.ColeccionTipo == "F") || txp.DistribucionxTalla.Count > 0 || pxc.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) >= 0)
                                                                    .Select(txp => new TallaViewModel
                                                                    {
                                                                        Talla = txp.CodigoTalla.ToUpper(),
                                                                        GrupoTallaId = txp.CodigoGrupoTalla,
                                                                        Orden = txp.Orden ?? 0,
                                                                        Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo && dis.Cantidad != ".00").Select(dis => new DistribucionXTallaViewModel
                                                                        {
                                                                            IdDistribucion = dis.IdDistribucion,
                                                                            IdTallaxGrupo = dis.IdTallaxGrupo,
                                                                            NombreDistribucion = dis.NombreDistribucion,
                                                                            NombreTalla = dis.NombreTalla,
                                                                            Cantidad = dis.Cantidad,
                                                                            Orden = dis.Orden,
                                                                        }).OrderBy(or => or.Orden).ToList(),

                                                                    }).OrderBy(txp => txp.Orden).ToList(),
                                                                    ListaColores = pxc.ColoresxProducto.Where(cpp => (vw_coleccion.ColeccionTipo == "F") || cpp.Colores.FisicoDisponible.Where(f => f.IdProducto == pxc.IdProducto).Sum(f => f.Disponible) > 0).OrderBy(cpp => cpp.Colores.Color).Select(cpp => new ColorViewModel
                                                                    {
                                                                        CodigoColor = cpp.Colores.CodigoColor,
                                                                        NombreColor = cpp.Colores.Color,
                                                                        Color = cpp.Colores.Rgb,
                                                                        ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null && txp.CodigoColor == cpp.Colores.CodigoColor).Select(foto => new FotografiasXProductoViewModel
                                                                        {
                                                                            IdFotografia = foto.IdFotografia,
                                                                            FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                                                            CodigoColor = foto.CodigoColor,
                                                                            Principal = foto.Principal ?? false
                                                                        }).ToList(),

                                                                    }).ToList(),
                                                                    ListaColoresSinStock = pxc.ColoresxProducto.Where(cpp => (vw_coleccion.ColeccionTipo == "F") || cpp.Colores.FisicoDisponible.Where(f => f.IdProducto == pxc.IdProducto).Sum(f => f.Disponible) == 0).OrderBy(cpp => cpp.Colores.Color).Select(cpp => new ColorSinStock
                                                                    {
                                                                        CodigoColor = cpp.Colores.CodigoColor,
                                                                        NombreColor = cpp.Colores.Color,
                                                                        Color = cpp.Colores.Rgb,
                                                                    }).ToList(),
                                                                    fisicaDisponible = pxc.FisicoDisponible
                                                                       /*.Where(f => (vw_coleccion.ColeccionTipo == "F") || f.Disponible >= 0)*/
                                                                       .Select(f => new FisicoDisponibleViewModel
                                                                       {
                                                                           CodigoColor = f.CodigoColor,
                                                                           IdTalla = f.CodigoTalla.ToUpper(),
                                                                           Cantidad = f.Disponible < 0 ? 0 : f.Disponible,
                                                                           MinStock = f.MinStock,
                                                                           PreciosEspecificos = f.PrecioEspecifico.Where(preEsp => filtarXGrupoPrecio).Where(preEsp => ListaGrupoPrecios.Contains(preEsp.GrupoPrecio)).Select(preEsp => new PrecioEspecificoViewModel
                                                                           {
                                                                               IdPrecioEspecifico = preEsp.IdPrecioEspecifico,
                                                                               IdMoneda = preEsp.IdMoneda,
                                                                               IdProducto = preEsp.IdProducto,
                                                                               GrupoPrecio = preEsp.GrupoPrecio,
                                                                               IdFisicoDisponible = preEsp.IdFisicoDisponible,
                                                                               Precio = preEsp.Hasta == new DateTime(1900, 1, 1) ? preEsp.Precio : 0,
                                                                           }).ToList(),
                                                                       }).ToList()
                                                                }).ToList()
                                                            }
                                             ).ToList()
                                         }).AsNoTracking().ToListAsync();

                        listaColecciones.AddRange(colecciones);
                    }

                    return Ok(listaColecciones);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpGet]
        [Route("~/api/catalago/productos/{codigoColeccion}/{empresa}")]
        public async Task<IHttpActionResult> ObtenerProductosCatalago(string codigoColeccion, string empresa)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    bool filtarXGrupoPrecio = true;
                    string urlImagenes = ctx.Configuraciones.FirstOrDefault(conf => conf.CodigoConfiguracion == "UrlImages")?.Valor ?? "";
                    List<ColeccionViewModel> listaColecciones = new List<ColeccionViewModel>();
                    List<string> ListaGrupoPrecios = new List<string>();
                    var paquetesBodegaEspecifico = await ctx.PaqueteBodegaEspecifico.Select(x => x.ColeccionId).ToListAsync();

                    var producto = ctx.ProductosxColeccion.Where(pxc => pxc.EmpresaId == empresa && pxc.Colecciones.CodigoColeccion == codigoColeccion && pxc.VisibleParaVentas == true).Select(pxc => new
                    {
                        CodigoProducto = pxc.CodigoProducto,
                        NombreProducto = pxc.NombreProducto,
                        ListaImagenes = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null)
                        .Where(txp => (pxc.FotografiasXProducto.Where(fxp => fxp.CodigoColor == "").Count() > 0 && txp.CodigoColor == "") || (pxc.FotografiasXProducto.FirstOrDefault() != null && txp.CodigoColor == pxc.FotografiasXProducto.FirstOrDefault().CodigoColor))
                        .OrderByDescending(foto => foto.Principal)
                        .Select(foto => new 
                        {
                            FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                            CodigoColor = foto.CodigoColor,
                            Principal = foto.Principal ?? false
                        }).ToList(),
                        ListaTalla = ctx.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto && txp.IdTallaxGrupo != null).Select(txp => txp.TallasXGrupo).Where(txp => false || (pxc.Colecciones.ColeccionTipo == "F") || txp.DistribucionxTalla.Count > 0 || pxc.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) >= 0)
                          .Select(txp => new 
                          {
                              Talla = txp.CodigoTalla.ToUpper(),
                              Orden = txp.Orden ?? 0,
                          }).OrderBy(txp => txp.Orden).ToList(),
                        ListaColores = pxc.ColoresxProducto.Where(cpp => (pxc.Colecciones.ColeccionTipo == "F") || cpp.Colores.FisicoDisponible.Where(f => f.IdProducto == pxc.IdProducto).Sum(f => f.Disponible) > 0).OrderBy(cpp => cpp.Colores.Color).Select(cpp => new 
                        {
                            CodigoColor = cpp.Colores.CodigoColor,
                            NombreColor = cpp.Colores.Color,
                            ImagenesPorColor = pxc.FotografiasXProducto.Where(txp => txp.FotografiaProducto != null && txp.CodigoColor == cpp.Colores.CodigoColor).Select(foto => new 
                            {
                                FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                CodigoColor = foto.CodigoColor,
                                Principal = foto.Principal ?? false
                            }).ToList(),

                        }).ToList(),
                        InventarioDisponible = pxc.FisicoDisponible
                        .Select(f => new 
                        {
                            CodigoColor = f.CodigoColor,
                            IdTalla = f.CodigoTalla.ToUpper(),
                            Cantidad = f.Disponible < 0 ? 0 : f.Disponible,
                            PreciosEspecificos = f.PrecioEspecifico.Where(preEsp => filtarXGrupoPrecio).Where(preEsp => ListaGrupoPrecios.Contains(preEsp.GrupoPrecio)).Select(preEsp => new PrecioEspecificoViewModel
                            {
                                IdPrecioEspecifico = preEsp.IdPrecioEspecifico,
                                IdMoneda = preEsp.IdMoneda,
                                IdProducto = preEsp.IdProducto,
                                GrupoPrecio = preEsp.GrupoPrecio,
                                IdFisicoDisponible = preEsp.IdFisicoDisponible,
                                Precio = preEsp.Hasta == new DateTime(1900, 1, 1) ? preEsp.Precio : 0,
                            }).ToList(),
                        }).ToList()
                    }).ToList();
                    return Ok(producto);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }


        [HttpPost]
        [Route("deshabilitarproducto")]
        public async Task<IHttpActionResult> DeshabilitarProducto([FromBody] DeshabilitarProducto producto)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    ProductosxColeccion productoBd = await ctx.ProductosxColeccion.FirstOrDefaultAsync(x => x.IdColeccion == producto.Coleccion && x.EmpresaId == producto.Pais.ToUpper() && x.CodigoProducto == producto.Producto);

                    if (productoBd == null)
                    {
                        return BadRequest("No se encuentra el producto.");
                    }

                    if (productoBd.VisibleParaVentas == false)
                    {
                        return BadRequest("El producto ya se encuentra deshabilitado.");
                    }

                    productoBd.VisibleParaVentas = false;
                    var res = await ctx.SaveChangesAsync();
                    return Ok(res);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("eliminarimagen/{codigocolor}")]
        public async Task<IHttpActionResult> EliminarImagen(int codigocolor)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    FotografiasXProducto fotografia = await ctx.FotografiasXProducto.FindAsync(codigocolor);

                    if (fotografia == null)
                    {
                        return BadRequest("La imagen de producto no existe.");
                    }

                    ctx.FotografiasXProducto.Remove(fotografia);
                    int res = await ctx.SaveChangesAsync();
                    return Ok(res);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [HttpPost]
        [Route("productoStock/{productoId}")]
        public async Task<IHttpActionResult> MostrarStock(int productoId)
        {
            try
            {
                using (AVentasEntities ctx = new AVentasEntities())
                {
                    ProductosxColeccion productoBd = await ctx.ProductosxColeccion.FirstOrDefaultAsync(x => x.IdProducto == productoId);

                    if (productoBd == null)
                    {
                        return BadRequest("No se encuentra el producto.");
                    }

                    productoBd.StockVisible = !productoBd.StockVisible;
                    var res = await ctx.SaveChangesAsync();
                    return Ok(productoBd.StockVisible);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        

        [HttpGet]
        [Route("~/api/colecciones/inventario/{codigoPaquete}")]
        public async Task<IHttpActionResult> ObtenerInventarioDisponible(string codigoPaquete)
        
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    var Inventario = await ctx.FisicoDisponible.Where(x => x.ProductosxColeccion.Colecciones.CodigoColeccion == codigoPaquete && x.ProductosxColeccion.EmpresaId == "IMHN" && x.ProductosxColeccion.VisibleParaVentas == true && x.Disponible > 0
                    && ctx.TallasxProducto.Where(cl => cl.IdProducto == x.IdProducto).Select(t => t.TallasXGrupo.CodigoTalla).Contains(x.CodigoTalla)
                    && ctx.ColoresxProducto.Where(cl => cl.IdProducto == x.IdProducto).Select(cl => cl.CodigoColor).Contains(x.CodigoColor)).Select(x => new
                    {
                        Producto = x.ProductosxColeccion.CodigoProducto,
                        Nombre_Producto = x.ProductosxColeccion.NombreProducto,
                        Color = x.CodigoColor,
                        Nombre_Color = x.Colores.Color,
                        Talla = x.CodigoTalla,
                        Inventario = x.Disponible
                    }).ToListAsync();

                    return Ok(Inventario);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}
