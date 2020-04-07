using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
//using AventasApi.GestorData;
using DBData.Database;
//using DBData.Database;
using AventasApi.Models.ViewModels;

using System.Data.Entity;
namespace AventasApi.Controllers
{
    public class ColeccionesXLineaController : ApiController
    {
        readonly AVentasEntities context = new AVentasEntities();

        public ColeccionesXLineaController()
        {
            this.context.Database.CommandTimeout = 300;
            this.context.Configuration.LazyLoadingEnabled = false;
        }

        [HttpGet]

        public async Task<IHttpActionResult> GetcoleccionesXGrupoPrecio(string id)
        {
            return Ok(ObtenerColecciones(id).Result);
        }
        [HttpGet]
        public async Task<IHttpActionResult> Getcolecciones()
        {
            return Ok(ObtenerColecciones(null).Result);
        }
        private async Task<List<ColeccionViewModel>> ObtenerColecciones(string grupoPrecio)
        {

            bool filtarXGrupoPrecio = grupoPrecio != null;
            //var atributosXColeccionList = context.vw_AtributosxColeccion.ToList();
            string urlImagenes = context.Configuraciones.FirstOrDefault(conf => conf.CodigoConfiguracion == "UrlImages")?.Valor ?? "";
            List<ColeccionViewModel> colecciones = await context.Colecciones
                //.Include(co=>co.EdadesxColeccion)
                .Where(vw_coleccion => vw_coleccion.VentaInicio <= DateTime.Today && vw_coleccion.VentaFinal >= DateTime.Today).OrderBy(vw_coleccion => vw_coleccion.VentaFinal).Select(vw_coleccion =>
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
                                   Descripcion = (atr.Descripcion2 == "BASE") ? atr.Descripcion1 + " - " + atr.CodigoAtributo : atr.Descripcion1,
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
                                                  ProductosXEdad = context.ProductosxColeccion.Where(pxc => pxc.IdColeccion == vw_coleccion.IdColeccion && pxc.IdEdad == me.IdEdad && me.IdLinea == pxc.IdLinea).Select(pxc => new ProductoXColeccionViewModel
                                                  {
                                                      ProductoId = pxc.CodigoProducto,
                                                      CodigoColeccion = vw_coleccion.CodigoColeccion,
                                                      CodigoProducto = pxc.IdProducto,
                                                      NombreProducto = pxc.NombreProducto,
                                                      Precio = pxc.PreciosxProducto.Where(preEsp => true || !filtarXGrupoPrecio).Select(precio => new PrecioXProductoViewModel
                                                      {
                                                          GrupoPrecio = precio.GrupoPrecio,
                                                          IdMoneda = precio.IdMoneda,
                                                          Precio = precio.Precio
                                                      }).ToList(),
                                                      GrupoTalla = pxc.CodigoGrupoTalla,
                                                      Linea = new LineaViewModel
                                                      {
                                                          IdLinea = pxc.MaestroLinea.IdLinea,
                                                          Linea = pxc.MaestroLinea.Linea,
                                                      },
                                                      AtributosXProducto = pxc.AtributosxProducto.Select(atr => new AtributosViewModel
                                                      {
                                                          Descripcion = (atr.Descripcion2 == "BASE") ? atr.Descripcion1 + " - " + atr.CodigoAtributo : atr.Descripcion1,
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
                                                      //ListaTalla = pxc.FisicoDisponible.GroupBy(fisDis => fisDis.CodigoTalla).Where(fisDisGroup => fisDisGroup.Sum(fisDis => fisDis.Disponible) > 0).Select(fisDisGroup => fisDisGroup.FirstOrDefault()).Select(fisDis => context.TallasXGrupo.Where(tallXGrup => tallXGrup.CodigoGrupoTalla == pxc.CodigoGrupoTalla).FirstOrDefault(tallXGrup => tallXGrup.CodigoTalla == fisDis.CodigoTalla)).Select(txp => new TallaViewModel
                                                      //{
                                                      //    Talla = txp.CodigoTalla,
                                                      //    GrupoTallaId = txp.CodigoGrupoTalla,
                                                      //    Orden = txp.Orden ?? 0,
                                                      //    Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo).Select(dis => new DistribucionXTallaViewModel
                                                      //    {
                                                      //        IdDistribucion = dis.IdDistribucion,
                                                      //        IdTallaxGrupo = dis.IdTallaxGrupo,
                                                      //        NombreDistribucion = dis.NombreDistribucion,
                                                      //        NombreTalla = dis.NombreTalla,
                                                      //        Cantidad = dis.Cantidad,
                                                      //    }).ToList(),

                                                      //}).OrderBy(txp => txp.Orden).ToList(),
                                                      ListaTalla = context.TallasxProducto.Where(txp => txp.IdProducto == pxc.IdProducto).Select(txp => txp.TallasXGrupo).Where(txp => false || (vw_coleccion.ColeccionTipo == "F") || txp.DistribucionxTalla.Count > 0 || pxc.FisicoDisponible.Where(f => f.CodigoTalla == txp.CodigoTalla).Sum(f => f.Disponible) > 0)
                                                      .Select(txp => new TallaViewModel
                                                      {
                                                          Talla = txp.CodigoTalla,
                                                          GrupoTallaId = txp.CodigoGrupoTalla,
                                                          Orden = txp.Orden ?? 0,
                                                          Distribucion = txp.DistribucionxTalla.Where(dis => dis.IdTallaxGrupo == txp.IdTallaxGrupo).Select(dis => new DistribucionXTallaViewModel
                                                          {
                                                              IdDistribucion = dis.IdDistribucion,
                                                              IdTallaxGrupo = dis.IdTallaxGrupo,
                                                              NombreDistribucion = dis.NombreDistribucion,
                                                              NombreTalla = dis.NombreTalla,
                                                              Cantidad = dis.Cantidad,
                                                          }).ToList(),

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
                                                         .Where(f => (vw_coleccion.ColeccionTipo == "F") || f.Disponible > 0)
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
                                                                 Precio = preEsp.Precio,
                                                             }).ToList(),
                                                         }).ToList()
                                                  }).ToList()
                                              }
                               ).ToList()
                           }).AsNoTracking().ToListAsync();
            //foreach (var coleccion in colecciones)
            //{
            //    coleccion.AtributosXColeccion = atributosXColeccionList.Where(atr => atr.IdColeccion == coleccion.IdColeccion).Select(atr => new AtributosViewModel
            //    {
            //        Descripcion = (atr.Descripcion2 == "BASE") ? atr.Descripcion1 + " - " + atr.CodigoAtributo : atr.Descripcion1,
            //        Tipo = atr.Descripcion2,
            //        IdLinea = atr.IdLinea
            //    }).ToList();
            //}
            return colecciones;
        }

    }
}
