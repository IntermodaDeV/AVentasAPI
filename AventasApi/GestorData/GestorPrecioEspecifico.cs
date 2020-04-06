using DBData.Database;
using AventasApi.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using System.Configuration;
using System.Data.SqlClient;
using BulkInsert;
using AventasApi.Enviroments;

namespace AventasApi.GestorData
{
    public class GestorPrecioEspecifico
    {
        private static string UrlString = $"{Enviroment.CRMWebServiceURLApi}paquetes/imhn/{{0}}/hnl/{{1}}";
        private static HttpClient client = new ClienteHttp();
        public static Task TaskActualizarLineas;

        //private static AVentasEntities context = new AVentasEntities();


        static GestorPrecioEspecifico()
        {
            ReiniciarTaskActualizarLineas();

        }
        public static async void ReiniciarTaskActualizarLineas()
        {


            TaskActualizarLineas = new Task(async () =>
            {

                List<Colecciones> colecciones = new List<Colecciones>();
                List<MaestroGrupoPrecio> gruposPrecio = new List<MaestroGrupoPrecio>();
                List<PrecioEspecifico2> preciosEspecificosAGuardar = new List<PrecioEspecifico2>();
                using (AVentasEntities context = new AVentasEntities())
                {
                    colecciones = context.Colecciones.Include(col => col.ProductosxColeccion.Select(pcc => pcc.FisicoDisponible)).AsNoTracking().ToList();
                    gruposPrecio = context.MaestroGrupoPrecio.AsNoTracking().ToList();
                }

                if (colecciones != null && colecciones.Count > 0)
                {
                    foreach (var col in colecciones)
                    {

                        var taskGetacuerdos =
                            gruposPrecio.Select(async gpp =>
                            {
                                List<PreciosCRMApiModel> facturasXCliente = new List<PreciosCRMApiModel>();
                                HttpResponseMessage response = await client.GetAsync(string.Format(UrlString,col.CodigoColeccion,gpp.GrupoPrecio)).ConfigureAwait(false);
                                if (response.IsSuccessStatusCode)
                                {
                                    try
                                    {
                                        facturasXCliente = await response.Content.ReadAsAsync<List<PreciosCRMApiModel>>();
                                        if (facturasXCliente == null)
                                        {
                                            facturasXCliente = new List<PreciosCRMApiModel>();
                                        }
                                        var preciosEspecificosXProducto = col.ProductosxColeccion.Select(pxc => new
                                        {
                                            idProducto = pxc.IdProducto,
                                            codigoPRoducto = pxc.CodigoProducto,
                                            listaPrecioEspecifico =
                                            pxc.FisicoDisponible.Select(fd => new { FisicoDisponible = fd, precioEspecifico = new PrecioEspecifico2 { IdFisicoDisponible = fd.IdFisicoDisponible, GrupoPrecio = gpp.GrupoPrecio, Precio = 0 } }).ToList()
                                        }).ToList();
                                        var precios = facturasXCliente.OrderByDescending(txg => txg.PRODUCT).GroupBy(txg => txg.PRODUCT)
                                        .Select(txg => new
                                        {
                                            producto = txg.Key, colores =
                                            txg.OrderByDescending(colo => colo.COLOR).GroupBy(colo => colo.COLOR).Select(colo =>new { color = colo.Key, tallas = 
                                            colo.OrderByDescending(tall => tall.SIZE).ToList() }).ToList()
                                        }).ToList();
                                        foreach (var precioXProducto in precios)
                                        {
                                            foreach (var colorXProducto in precioXProducto.colores)
                                            {
                                                foreach (var tallaxpProducto in colorXProducto.tallas)
                                                {
                                                    for (int i = preciosEspecificosXProducto.Count() - 1; i >= 0; i--)
                                                    {
                                                        if (precioXProducto.producto.Contains("++") || preciosEspecificosXProducto[i].codigoPRoducto == precioXProducto.producto)
                                                        {
                                                            for (int j = preciosEspecificosXProducto[i].listaPrecioEspecifico.Count - 1; j >= 0; j--)
                                                            {
                                                                var elementoListaPRecioEspecifico = preciosEspecificosXProducto[i].listaPrecioEspecifico[j];
                                                                if (colorXProducto.color.Contains("++") || elementoListaPRecioEspecifico.FisicoDisponible.CodigoColor == colorXProducto.color)
                                                                {
                                                                    if (tallaxpProducto.SIZE.Contains("++") || elementoListaPRecioEspecifico.FisicoDisponible.CodigoTalla == tallaxpProducto.SIZE)
                                                                    {
                                                                        try
                                                                        {
                                                                            elementoListaPRecioEspecifico.precioEspecifico.Precio = Decimal.Parse(tallaxpProducto.PRICE);
                                                                        }
                                                                        catch (Exception)
                                                                        { }
                                                                        lock (preciosEspecificosAGuardar)
                                                                        {
                                                                            preciosEspecificosAGuardar.Add(elementoListaPRecioEspecifico.precioEspecifico);
                                                                        }
                                                                        preciosEspecificosXProducto[i].listaPrecioEspecifico.RemoveAt(j);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                        }



                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine(e);
                                    }

                                }
                                else
                                {
                                    Debug.WriteLine("Error en a peticion");
                                }

                            });
                        await Task.WhenAll(taskGetacuerdos);
                    }
                    using (AVentasEntities context = new AVentasEntities())
                    {

                        var connectionString = "data source=209.126.64.158,49170;initial catalog=AventasTesting20200211;persist security info=True;user id=developer;password=D3vCitHn.20!8;MultipleActiveResultSets=True;App=EntityFramework&quot;";


                        var transaction = context.Database.BeginTransaction();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.BulkInsert("PrecioEspecifico", preciosEspecificosAGuardar);
                    }
                    transaction.Commit();
                       
                    }
                    Debug.WriteLine("FFinalizo");
                }
            });
        }
    }
    public partial class PrecioEspecifico2
    {
        public int IdPrecioEspecifico { get; set; }
        public string IdMoneda { get; set; }
        public Nullable<int> IdProducto { get; set; }
        public string GrupoPrecio { get; set; }
        public Nullable<int> IdFisicoDisponible { get; set; }
        public Nullable<decimal> Precio { get; set; }
    }
}