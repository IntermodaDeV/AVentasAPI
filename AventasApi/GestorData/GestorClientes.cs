using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Enviroments;
using AventasApi.Infrastructure;
using AventasApi.Models;
using AventasApi.Models.ApiModels;
using AventasApi.Models.ViewModels;
using Newtonsoft.Json;
using RestSharp;

namespace AventasApi.GestorData
{
    public class GestorClientes
    {
        private  string UrlString = $"{Enviroment.CRMWebServiceURLApi}clientes/{0}/{1}";


        public async Task<ClientesYMaestroGrupoPrecioViewModel> ObtenerClientesConRutaYMaestroGrupoPrecio(List<Rutas> rutasAsesor)
        {
            List<Asesores> asesores = new List<Asesores>();
            List<Clientes> clientesConRuta = new List<Clientes>();
            List<MaestroGrupoPrecio> gruposPrecio = new List<MaestroGrupoPrecio>();
            List<Rutas> rutas = rutasAsesor;

            using (AVentasEntities context = new AVentasEntities())
            {
                asesores = context.Asesores.AsNoTracking().ToList();
            }
            Parallel.ForEach(asesores, ase =>
            {
               
                string peticion = string.Format(UrlString, ase.EmpresaId, ase.Usuario);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    var clientes = JsonConvert.DeserializeObject<List<ClientesCRMApiModel>>(response.Content);
                    if (clientes == null)
                        clientes = new List<ClientesCRMApiModel>();
                    foreach (var cliente in clientes)
                    {
                        var clienteAAgregar = new Clientes
                        {
                            CodigoCliente = cliente.ACCOUNT,
                            EmpresaId = cliente.ENTITY,
                            Nombre = cliente.NAME,
                            ComunidadAutonoma = cliente.AUTONOMOUS_COMMUNITY,
                            GrupoPrecio = cliente.PRICE,
                            GrupoCliente = cliente.CUSTOMER_GROUP,
                            Descuento = cliente.TOTAL_DISCOUNT,
                            Direccion = cliente.ADDRESS,
                            IdMoneda = cliente.CURRENCY,
                            FacturacionEntrega = cliente.BLOCKED,
                            //Latitud = ,
                            //Longitud = ,
                            //Provincias = ,
                            //Region = ,
                            //Revision = ,

                        };
                        if (!gruposPrecio.Any(gp => gp.GrupoPrecio == cliente.PRICE))
                        {
                            lock (gruposPrecio)
                            {
                                gruposPrecio.Add(new MaestroGrupoPrecio
                                {
                                    GrupoPrecio = cliente.PRICE,
                                    Descripcion = cliente.PRICE_NAME
                                });
                            }
                        }
                        var rutaAAgregar = new Rutas
                        {
                            CodigoRuta = cliente.ENTITY.ToLower()  + "-" + cliente.SALES_AREA,
                            EmpresaId = cliente.ENTITY,
                            Nombre = cliente.SALES_AREA_NAME
                        };
                        lock (rutas)
                        {
                            if (!rutas.Any(rutXAgre => rutXAgre.CodigoRuta == rutaAAgregar.CodigoRuta))
                            {
                                rutas.Add(rutaAAgregar);
                            }
                        }
                        try
                        {
                            clienteAAgregar.CreditoDisponible = decimal.Parse(cliente.CREDIT_AVAILABLE);
                        }
                        catch (Exception) { }
                        try
                        {
                            clienteAAgregar.LimiteCredito = decimal.Parse(cliente.CREDIT_LIMIT);
                        }
                        catch (Exception) { }
                        clienteAAgregar.ClientesxRuta.Add(new ClientesxRuta
                        {
                            CodigoRuta = cliente.ENTITY + "-" + cliente.SALES_AREA,
                            CodigoCliente = cliente.ACCOUNT,
                        });
                        lock (clientesConRuta)
                        {
                            clientesConRuta.Add(clienteAAgregar);
                        }
                    }
                }
            }
            );

            return new ClientesYMaestroGrupoPrecioViewModel
            {
                ClientesConRuta = clientesConRuta,
                MaestroGrupoPrecio = gruposPrecio,
                Rutas = rutas
            };
        }
        public async Task GuardarRutas(List<Rutas> rutasAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Rutas.AddRange(rutasAGuardar.OrderBy(rut => rut.EmpresaId).ThenBy(rut => rut.CodigoRuta));
                await context.SaveChangesAsync();
            }
        }
        public async Task GuardarClientesConRuta(List<Clientes> clientesAAGregar)
        {
            clientesAAGregar.ForEach(asa => Debug.WriteLine(asa.ClientesxRuta.First().CodigoRuta));
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Clientes.AddRange(clientesAAGregar);
                await context.SaveChangesAsync();
            }
        }
        public async Task GuardarGrupoPrecio(List<MaestroGrupoPrecio> gruposPrecioAAgregar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.MaestroGrupoPrecio.AddRange(gruposPrecioAAgregar);
                await context.SaveChangesAsync();
            }
        }
    }
}