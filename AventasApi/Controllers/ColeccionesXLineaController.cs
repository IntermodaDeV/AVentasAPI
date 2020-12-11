using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DBData.Database;
using AventasApi.Models.ViewModels;

using System.Data.Entity;
using AventasApi.Models;

namespace AventasApi.Controllers
{
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
                .Where(vw_coleccion => vw_coleccion.VentaInicio <= DateTime.Today && vw_coleccion.VentaFinal >= DateTime.Today && vw_coleccion.EmpresaId.ToUpper()==pais.ToUpper()).OrderBy(vw_coleccion => vw_coleccion.VentaFinal).Select(vw_coleccion =>
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
                                                  ProductosXEdad = context.ProductosxColeccion.Where(pxc => pxc.EmpresaId==pais.ToUpper() && pxc.IdColeccion == vw_coleccion.IdColeccion && pxc.IdEdad == me.IdEdad && me.IdLinea == pxc.IdLinea && pxc.VisibleParaVentas == true).Select(pxc => new ProductoXColeccionViewModel
                                                  {
                                                      ProductoId = pxc.CodigoProducto,
                                                      CodigoColeccion = vw_coleccion.CodigoColeccion,
                                                      CodigoProducto = pxc.IdProducto,
                                                      NombreProducto = pxc.NombreProducto,
                                                      GrupoImpuesto=(string.IsNullOrEmpty(pxc.GrupoImpuesto))?"GENERAL":pxc.GrupoImpuesto.ToUpper(),
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
        [Route("~/api/colecciones/{linea}/{pais}")]
        public async Task<IHttpActionResult> GetColeccionesPorLinea(string linea,string pais)
        {
            try
            {
                using (var ctx = new AVentasEntities())
                {
                    List<ColeccionViewModel> colecciones = await ctx.Colecciones
                        .Where(vw_coleccion => vw_coleccion.VentaInicio <= DateTime.Today
                               && vw_coleccion.VentaFinal >= DateTime.Today
                               && vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper()
                               && vw_coleccion.LineasxColeccion.Select(x => x.IdLinea).Contains(linea))
                        .OrderBy(vw_coleccion => vw_coleccion.VentaFinal)
                        .Select(vw_coleccion =>
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
                                 }).ToList()
                             }).ToListAsync();

                    return Ok(colecciones);
                }
            }catch(Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("~/api/colecciones/productos/{coleccion}/{grupoprecio}/{pais}")]
        public async Task<IHttpActionResult> GetProductosPorColeccion(string coleccion,string grupoprecio, string pais)
        {
            try
            {
                using(var ctx = new AVentasEntities())
                {
                    string urlImagenes = ctx.Configuraciones.FirstOrDefault(conf => conf.CodigoConfiguracion == "UrlImages")?.Valor ?? "";
                    bool filtarXGrupoPrecio = grupoprecio != null;
                    List<ColeccionViewModel> colecciones = await ctx.Colecciones
                        .Where(vw_coleccion =>vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper()&& vw_coleccion.CodigoColeccion==coleccion.ToUpper())
                        .Select(vw_coleccion =>
                             new ColeccionViewModel
                             {
                                Edades = vw_coleccion.EdadesxColeccion
                               .OrderBy(me => me.MaestroEdad.Orden).Select(me => new EdadesViewModel
                                              {
                                                  IdEdad = me.IdEdad,
                                                  Edad = me.MaestroEdad.Edad,
                                                  Orden = me.MaestroEdad.Orden,
                                                  ProductosXEdad = ctx.ProductosxColeccion.Where(pxc => pxc.EmpresaId == pais.ToUpper() && pxc.IdColeccion == vw_coleccion.IdColeccion && pxc.IdEdad == me.IdEdad && pxc.IdLinea==me.IdLinea && pxc.VisibleParaVentas == true).Select(pxc => new ProductoXColeccionViewModel
                                                  {
                                                      ProductoId = pxc.CodigoProducto,
                                                      CodigoColeccion = vw_coleccion.CodigoColeccion,
                                                      CantidadMinima = pxc.CantidadMinima==null?0:pxc.CantidadMinima,
                                                      CodigoProducto = pxc.IdProducto,
                                                      NombreProducto = pxc.NombreProducto,
                                                      GrupoImpuesto = (string.IsNullOrEmpty(pxc.GrupoImpuesto)) ? "GENERAL" : pxc.GrupoImpuesto.ToUpper(),
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
                                                               CodigoColor = foto.CodigoColor,
                                                               Principal = foto.Principal ?? false
                                                           }).ToList(),
                                                      ListaTalla = ctx.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto).Select(txp => txp.TallasXGrupo).Where(txp => false || (vw_coleccion.ColeccionTipo == "F") || txp.DistribucionxTalla.Count > 0 || pxc.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) >= 0)
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
                                                               PreciosEspecificos = f.PrecioEspecifico.Where(preEsp => filtarXGrupoPrecio).Where(preEsp => preEsp.GrupoPrecio == grupoprecio).Select(preEsp => new PrecioEspecificoViewModel
                                                               {
                                                                   IdPrecioEspecifico = preEsp.IdPrecioEspecifico,
                                                                   IdMoneda = preEsp.IdMoneda,
                                                                   IdProducto = preEsp.IdProducto,
                                                                   GrupoPrecio = preEsp.GrupoPrecio,
                                                                   IdFisicoDisponible = preEsp.IdFisicoDisponible,
                                                                   Precio = preEsp.Hasta == new DateTime(1900, 1, 1) ? preEsp.Precio : pxc.PreciosxProducto.FirstOrDefault(pre =>pre.GrupoPrecio == grupoprecio && pre.IdProducto==preEsp.IdProducto).Precio,
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

                    foreach (var grupoPrecio in grupo.ListaPrecios)
                    {
                        foreach (var pais in grupo.Paises)
                        {
                            var existe = ctx.MaestroGrupoPrecio.FirstOrDefault(x => x.GrupoPrecio == grupoPrecio && x.EmpresaId == pais);
                            if (existe != null)
                            {
                                List<ColeccionViewModel> colecciones = await ctx.Colecciones
                                    .Where(vw_coleccion => vw_coleccion.VentaInicio <= DateTime.Today && vw_coleccion.VentaFinal >= DateTime.Today && vw_coleccion.EmpresaId.ToUpper() == pais.ToUpper()).OrderBy(vw_coleccion => vw_coleccion.VentaFinal).Select(vw_coleccion =>
                                                 new ColeccionViewModel
                                                 {
                                                     GrupoPrecio = grupoPrecio,
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
                                                                        ProductosXEdad = ctx.ProductosxColeccion.Where(pxc => pxc.EmpresaId == pais.ToUpper() && pxc.IdColeccion == vw_coleccion.IdColeccion && pxc.IdEdad == me.IdEdad && me.IdLinea == pxc.IdLinea && pxc.VisibleParaVentas == true).Select(pxc => new ProductoXColeccionViewModel
                                                                        {
                                                                            ProductoId = pxc.CodigoProducto,
                                                                            CantidadMinima = pxc.CantidadMinima == null ? 0 : pxc.CantidadMinima,
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
                                                                               .Where(txp => (pxc.FotografiasXProducto.Where(fxp => fxp.CodigoColor == "").Count() > 0 && txp.CodigoColor == "") || (pxc.FotografiasXProducto.FirstOrDefault() != null && txp.CodigoColor == pxc.FotografiasXProducto.FirstOrDefault().CodigoColor))
                                                                               .OrderByDescending(foto => foto.Principal)
                                                                               .Select(foto => new FotografiasXProductoViewModel
                                                                               {
                                                                                   IdFotografia = foto.IdFotografia,
                                                                                   FotografiaProducto = urlImagenes + foto.FotografiaProducto,
                                                                                   CodigoColor = foto.CodigoColor,
                                                                                   Principal = foto.Principal ?? false
                                                                               }).ToList(),
                                                                            ListaTalla = ctx.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto).Select(txp => txp.TallasXGrupo).Where(txp => false || (vw_coleccion.ColeccionTipo == "F") || txp.DistribucionxTalla.Count > 0 || pxc.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) >= 0)
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

                                listaColecciones.AddRange(colecciones);
                            }
                        }
                    }

                    return Ok(listaColecciones);
                }
            }catch(Exception e)
            {
                return BadRequest();
            }
        }
    }
}
