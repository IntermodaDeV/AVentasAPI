using AventasApi.GestorData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using AventasApi.Services.AsyncJobs;

namespace AventasApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GestorAsesores ga = new GestorAsesores();
            GestorMaestroRutas gestorRutas = new GestorMaestroRutas();
            GestorClientes gestorClientes = new GestorClientes();
            GestorRutasXAsesor gestorRutasXAsesor = new GestorRutasXAsesor();
            GestorImagenesXProducto gestorImagenesXProducto = new GestorImagenesXProducto();
            GestorAcuerdosVenta gestorAcuerdosVenta = new GestorAcuerdosVenta();
            GestorColecciones2 gestorColecciones = new GestorColecciones2();
            GestorSizesByProduct gestorSizesByProduct = new GestorSizesByProduct();


            //var listaAsesores = ga.ObtenerAsesores().Result;
            //ga.GuardarAsesores(listaAsesores).Wait();

            {
                //var llstaRutas = gestorRutas.ObtenerRutas().Result;



                //var clientesConRutaYMaestroGrupoPrecio = gestorClientes.ObtenerClientesConRutaYMaestroGrupoPrecio(llstaRutas).Result;

                //gestorClientes.GuardarGrupoPrecio(clientesConRutaYMaestroGrupoPrecio.MaestroGrupoPrecio).Wait();
                //gestorClientes.GuardarRutas(clientesConRutaYMaestroGrupoPrecio.Rutas).Wait();
                //var llstaRutasXAsesor = gestorRutasXAsesor.ObtenerRutasXAsesor().Result;
                //gestorRutasXAsesor.GuardarRutasXAsesor(llstaRutasXAsesor).Wait();
                //gestorClientes.GuardarClientesConRuta(clientesConRutaYMaestroGrupoPrecio.ClientesConRuta).Wait();
            }

            //var listIamgenesXProducto = gestorImagenesXProducto.ObtenerImagenesXProducto().Result;
            //gestorImagenesXProducto.GuardarImagenesXProducto(listIamgenesXProducto).Wait();

            //var listAcuerdos = gestorAcuerdosVenta.ObtenerAcuerdosxCliente().Result;
            //gestorAcuerdosVenta.GuardarAcuerdos(listAcuerdos).Wait();

            //var coleccionesYTiposDeColeccion = gestorColecciones.ObtenerColecciones().Result;
            //gestorColecciones.GuardarTiposDeColecciones(coleccionesYTiposDeColeccion.TiposdeColeccion).Wait();
            //gestorColecciones.GuardarColecciones(coleccionesYTiposDeColeccion.Colecciones).Wait();







            //GestorMaestroEdad.ActualizarProductos();
            //GestorProductos.ActualizarProductos();

            //GestorClientes.ActualizarClientes();
            //GestorAsesores.ActualizarAsesores();
            //GestorImagenesXProducto.ActualizarImagenes();
            GestorSizesByProduct.ObtenerTallasXProducto();

            if (false && GestorSubFacturasXCliente.TaskActualizarLineas.Status != TaskStatus.Running)
            {
                try
                {
                    if (GestorSubFacturasXCliente.TaskActualizarLineas.Status != TaskStatus.Created)
                    {
                        GestorSubFacturasXCliente.TaskActualizarLineas.Start();
                    }
                    GestorSubFacturasXCliente.TaskActualizarLineas.Start();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }

            }
            //if (false && GestorColecciones.TaskActualizarColecciones.Status != TaskStatus.Running)
            //{
            //    try
            //    {
            //        if (true && GestorColecciones.TaskActualizarColecciones.Status != TaskStatus.Created)
            //        {
            //            GestorColecciones.TaskActualizarColecciones.Start();
            //        }

            //        GestorColecciones.TaskActualizarColecciones.Start();
            //    }
            //    catch (Exception e)
            //    {
            //        Debug.WriteLine(e);
            //        throw;
            //    }

            //}

            var factory = AsyncSqlInsert.factory;
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
