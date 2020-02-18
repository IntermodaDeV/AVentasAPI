using AventasApi.Filters;
using AventasApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AventasApi.Infrastructure;
using AventasApi.Models.ViewModels;
using AventasApi.GestorData;

namespace AventasApi.Controllers
{

    public class ColeccionController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        public ColeccionController()
        {
            this.context.Database.CommandTimeout = 300;
        }
        [HttpGet]
        public async Task<IHttpActionResult> Getcolecciones()

        {
            //if (GestorDatas.colecicion != null && false)
            //{
            //    return Ok(GestorDatas.colecicion);
            //}
            List<ColeccionViewModel> colecciones = context.Colecciones.OrderBy(vw_coleccion => vw_coleccion.VentaFinal).Select(vw_coleccion =>
                         new ColeccionViewModel
                         {
                             CodigoColeccion = vw_coleccion.CodigoColeccion,
                             Nombre = vw_coleccion.Nombre,
                             ColeccionTipo = vw_coleccion.ColeccionTipo,
                             EmpresaId = vw_coleccion.EmpresaId,
                             DisenoInicio = vw_coleccion.DisenoInicio,
                             DisenoFinal = vw_coleccion.DisenoFinal,
                             EntregaInicio = vw_coleccion.EntregaInicio,
                             EntregaFinal = vw_coleccion.EntregaFinal,
                             Estatus = vw_coleccion.Estatus ?? 0,
                             ProduccionInicio = vw_coleccion.ProduccionInicio,
                             ProduccionFinal = vw_coleccion.ProduccionFinal,
                             VentaInicio = vw_coleccion.VentaInicio,
                             VentaFinal = vw_coleccion.VentaFinal,
                             //AtributosXColeccion = context.AtributosXProducto.Where(atr => context.ProductosxColeccion.FirstOrDefault(pxc => pxc.Id == atr.CodigoProducto).CodigoColeccion == vw_coleccion.CodigoColeccion).Select(atr => new AtributosViewModel
                             //{
                             //    Descripcion = atr.Descripcion1,
                             //    Tipo = atr.Descripcion2
                             //}).Distinct().ToList(),
                             productos = context.ProductosxColeccion.Where(pxc => pxc.IdColeccion == vw_coleccion.CodigoColeccion).Select(pxc => new ProductoXColeccionViewModel
                             {
                                 ProductoId = pxc.CodigoProducto,
                                 CodigoColeccion = pxc.CodigoColeccion,
                                 CodigoProducto = pxc.Id,
                                 NombreProducto = pxc.NombreProducto,
                                 Precio = pxc.Precio,
                                 Linea = context.MaestroLinea.Select(ml => new LineaViewModel
                                 {
                                     IdLinea = ml.IdLinea,
                                     Linea = ml.Linea
                                 }).FirstOrDefault(ml => ml.IdLinea == pxc.IdLinea),
                                 AtributosXProducto = context.AtributosXProducto.Where(atr => atr.CodigoProducto == pxc.Id).Select(atr => new AtributosViewModel
                                 {
                                     Descripcion = atr.Descripcion1,
                                     Tipo = atr.Descripcion2
                                 }).ToList(),
                                 ListaImagenes = context.FotografiasxProducto.Where(txp => txp.CodigoProducto == pxc.Id && txp.FotografiaProducto != null).Select(foto => foto.FotografiaProducto).ToList(),
                                 ListaTalla = context.TallasxProducto.Where(txp => txp.CodigoProducto == pxc.Id).Select(txp => new TallaViewModel
                                 {
                                     Talla = txp.CodigoTalla,
                                     GrupoTallaId = txp.GrupoTallaId,
                                     Orden = txp.Orden??0
                                 }).OrderBy(txp=> txp.Orden).ToList(),
                                 ListaColores = context.ColoresxProducto.Where(col => col.CodigoProducto == pxc.Id).Select(col => context.Colores.Select(color => new ColorViewModel
                                 {
                                     CodigoColor = color.CodigoColor,
                                     NombreColor = color.NombreColor,
                                     Color = color.Color

                                 }).FirstOrDefault(color => col.CodigoColor == color.CodigoColor)).ToList(),
                                 fisicaDisponible = context.FisicoDisponible.Where(col => col.CodigoProducto == pxc.Id).Select(f => new FisicoDisponibleViewModel
                                 {
                                     CodigoColor = f.CodigoColor,
                                     IdTalla = f.CodigoTalla,
                                     Cantidad = f.Disponible,
                                     MinStock = f.MinStock
                                 }).ToList()
                             }).ToList()
                         }).ToList();
            //GestorDatas.colecicion = colecciones;
            return Ok(colecciones);

        }

    }
}
