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
    public class PruebaColeccionesXLineaController : ApiController
    {
        AVentasEntities context = new AVentasEntities();

        public PruebaColeccionesXLineaController()
        {
            this.context.Database.CommandTimeout = 300;
            //this.context.Configuration.LazyLoadingEnabled = false;
            this.context.Configuration.LazyLoadingEnabled = true;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Getcolecciones()
        {
            return Ok("Controlador modificado");
            //var atributosXColeccionList = context.vw_AtributosxColeccion.ToList();
            //AtributosXColeccion = context.vw_AtributosxColeccion.Where(atr => atr.IdColeccion == vw_coleccion.IdColeccion).Select(atr => new AtributosViewModel
            //{
            //    Descripcion = (atr.Descripcion2 == "BASE") ? atr.Descripcion1 + " - " + atr.CodigoAtributo : atr.Descripcion1,
            //    Tipo = atr.Descripcion2,
            //    IdLinea = atr.IdLinea
            //}).ToList(),
            List<ColeccionViewModel> colecciones = new List<ColeccionViewModel>();
            var coleccionesDB = context.Colecciones.Where(vw_coleccion => vw_coleccion.VentaInicio <= DateTime.Today && vw_coleccion.VentaFinal >= DateTime.Today).OrderBy(vw_coleccion => vw_coleccion.VentaFinal);
            Parallel.ForEach(coleccionesDB, coleccionDB =>
            {
                ColeccionViewModel coleccion = new ColeccionViewModel
                {
                    IdColeccion = coleccionDB.IdColeccion,
                    CodigoColeccion = coleccionDB.CodigoColeccion,
                    Nombre = coleccionDB.Nombre,
                    ColeccionTipo = coleccionDB.ColeccionTipo,
                    EmpresaId = coleccionDB.EmpresaId,
                    FotoPortada = coleccionDB.FotoPortada,
                    DisenoInicio = coleccionDB.DisenoInicio,
                    DisenoFinal = coleccionDB.DisenoFinal,
                    EntregaInicio = coleccionDB.EntregaInicio,
                    EntregaFinal = coleccionDB.EntregaFinal,
                    Estatus = coleccionDB.Estatus ?? 0,
                    ProduccionInicio = coleccionDB.ProduccionInicio,
                    ProduccionFinal = coleccionDB.ProduccionFinal,
                    VentaInicio = coleccionDB.VentaInicio,
                    VentaFinal = coleccionDB.VentaFinal,
                    Lineas = coleccionDB.LineasxColeccion.Select(colXLin => colXLin.IdLinea).ToList(),
                    Edades = new List<EdadesViewModel>()
                };
                Parallel.ForEach(coleccionDB.EdadesxColeccion, edadXColeccion =>
                {
                    lock (coleccion.Edades)
                    {
                        coleccion.Edades.Add(new EdadesViewModel
                        {
                            IdEdad = edadXColeccion.IdEdad,
                            Edad = edadXColeccion.MaestroEdad.Edad,
                            Orden = edadXColeccion.MaestroEdad.Orden
                        });
                    }
                });
                lock (colecciones)
                {
                    colecciones.Add(coleccion);
                }
            });


            return Ok(coleccionesDB);
        }
    }
}
