using DBData.Database;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.ApiModels;
using AventasApi.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AventasApi.Utils
{
    public class SyncClientes
    {
        public void SyncCliente(string empresa, string codigoCliente, string codigoAsesor)
        {
            try
            {
                var clientes = new List<ClientesCRMApiModel>();
                var resClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                var request = new RestRequest($"clientes/{empresa}/{codigoAsesor}/{codigoCliente}", Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = resClient.Execute(request);

                if (response.IsSuccessful && response.Content != "null")
                {
                    clientes = JsonConvert.DeserializeObject<List<ClientesCRMApiModel>>(response.Content);
                }

                if (clientes.Count > 0)
                {
                    UpdateCliente(clientes);
                }

            }
            catch (Exception e)
            {

            }
        }

        private void UpdateCliente(List<ClientesCRMApiModel> cliente)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    foreach (var cli in cliente)
                    {
                        var entityFound = db.Clientes.FirstOrDefault(x=> x.CodigoCliente == cli.ACCOUNT && x.EmpresaId == cli.ENTITY);
                        decimal cLimite = 0, cDisponible = 0;

                        if (entityFound != null)
                        {
                            entityFound.CodigoCliente = cli.ACCOUNT;
                            entityFound.EmpresaId = cli.ENTITY;
                            entityFound.Nombre = cli.NAME;
                            entityFound.Zona = cli.SALES_AREA;
                            entityFound.ComunidadAutonoma = cli.AUTONOMOUS_COMMUNITY;
                            entityFound.GrupoPrecio = cli.PRICE;
                            entityFound.GrupoCliente = cli.CUSTOMER_GROUP;
                            //entityFound.Descuento = cli.DISCOUNT_GROUP;
                            entityFound.Direccion = cli.ADDRESS;
                            entityFound.IdMoneda = cli.CURRENCY;
                            entityFound.FacturacionEntrega = cli.BLOCKED;
                            //entityFound.IncluyeImpuesto = cli.INCLUDE_TAX == "Sí";
                            entityFound.Provincias = null;
                            entityFound.Region = null;
                            entityFound.Revision = null;
                            entityFound.LimiteCredito = Decimal.TryParse(cli.CREDIT_LIMIT, out cLimite) ? cLimite : 0;
                            entityFound.CreditoDisponible = Decimal.TryParse(cli.CREDIT_LIMIT, out cDisponible) ? cDisponible : 0;
                            //entityFound.ModoEntrega = cli.DLVMODE;
                            entityFound.Telefono = cli.PHONE;
                            entityFound.GrupoImpuesto = cli.TAX_GROUP;
                            entityFound.Habilitado = true;
                            //entityFound.IgnorarSeqFact = cli.FLAG_SEQFACT == "Sí" ? true : false;


                            db.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                        }
                       
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }



        public async Task SincronizacionClientes(string codigoAsesor)
        {
            try
            {
                var asesores = await VerificarAsesor(codigoAsesor);

                foreach(var asesor in asesores)
                {
                    var clientes = new List<ClientesCRMApiModel>();
                    var restClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                    var request = new RestRequest($"clientes/{asesor.EmpresaId}/{asesor.Usuario}", Method.GET);
                    request.Timeout = 5 * 60000;
                    request.AddHeader("Accept", "application/json");
                    IRestResponse response = restClient.Execute(request);

                    if (response.IsSuccessful && response.Content != "null")
                    {
                        clientes = JsonConvert.DeserializeObject<List<ClientesCRMApiModel>>(response.Content);

                        if (clientes.Count > 0)
                        {
                            LimpiarClientes(asesor.CodigoAsesor, asesor.EmpresaId);
                            await NewAndUpdateCliente(clientes, asesor.EmpresaId);
                        }
                    }
                }                    
            }
            catch (Exception e)
            {
                
            }
        }

        private async Task NewAndUpdateCliente(List<ClientesCRMApiModel> clientes, string EmpresaId)
        {
            try
            {
                foreach (var cliente in clientes)
                {                    
                    MaestroGrupoPrecio maestroGrupoPrecio = new MaestroGrupoPrecio
                    {
                        EmpresaId = cliente.ENTITY,
                        GrupoPrecio = cliente.PRICE,
                        Descripcion = cliente.PRICE_NAME
                    };
                     await SincronizarMaestroGrupoPrecio(maestroGrupoPrecio);

                    decimal cLimite = 0, cDisponible = 0;
                    Clientes clientesModel = new Clientes
                    {
                        CodigoCliente = cliente.ACCOUNT,
                        EmpresaId = cliente.ENTITY,
                        Nombre = cliente.NAME,
                        Zona = cliente.SALES_AREA,
                        ComunidadAutonoma = cliente.AUTONOMOUS_COMMUNITY,
                        GrupoPrecio = cliente.PRICE,
                        GrupoCliente = cliente.CUSTOMER_GROUP,
                        Descuento = cliente.DISCOUNT_GROUP == null ? " " : cliente.DISCOUNT_GROUP,
                        Direccion = cliente.ADDRESS,
                        IdMoneda = cliente.CURRENCY,
                        FacturacionEntrega = cliente.BLOCKED,
                        IncluyeImpuesto = cliente.INCLUDE_TAX == "Sí",                        
                        Provincias = null,
                        Region = null,
                        Revision = null,
                        LimiteCredito = Decimal.TryParse(cliente.CREDIT_LIMIT, out cLimite) ? cLimite : 0,
                        CreditoDisponible = Decimal.TryParse(cliente.CREDIT_LIMIT, out cDisponible) ? cDisponible : 0,
                        ModoEntrega = cliente.DLVMODE,
                        Telefono = cliente.PHONE,
                        GrupoImpuesto = cliente.TAX_GROUP,
                        CodigoAsesor = cliente.VENDOR,
                        Habilitado = true,
                        IgnorarSeqFact = cliente.FLAG_SEQFACT == "Sí" ? true : false,
                        FlagClienteEspecial = cliente.SPECIALCUSTOMER == "Sí" ? true : false,
                        Departamento = cliente.COUNTY,
                        Municipio=cliente.CITY,
                        Alias=cliente.ALIAS,
                        DiasTransporte=cliente.TRANSPORTDAY
                    };
                     await SincronizarClientes(clientesModel);

                    ClientesxRuta clientesRutas = new ClientesxRuta
                    {
                        CodigoCliente = cliente.ACCOUNT,
                        CodigoRuta = EmpresaId + '-' + cliente.SALES_AREA,
                    };
                     await SyncClientesxRutas(clientesRutas);
                }
            }
            catch (Exception e)
            {

               
            }
        }

        private async Task SyncClientesxRutas(ClientesxRuta ruta)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
            {
                var entity = await db.ClientesxRuta.FirstOrDefaultAsync(p => p.Rutas.CodigoRuta == ruta.CodigoRuta && p.Clientes.CodigoCliente == ruta.CodigoCliente);
                if(entity == null)
                {
                        var rutaDB = await db.Rutas.FirstOrDefaultAsync(x => x.CodigoRuta == ruta.CodigoRuta);
                        var clienteDB = await db.Clientes.FirstOrDefaultAsync(x => x.CodigoCliente == ruta.CodigoCliente);

                        if(rutaDB!=null && clienteDB != null)
                        {
                            ruta.Rutas = rutaDB;
                            ruta.Clientes = clienteDB;
                            db.ClientesxRuta.Add(ruta);
                            await db.SaveChangesAsync();
                        }                        
                }
                
            }
            }
            catch (Exception e)
            {
               
            }
            
        }

        private async Task SincronizarClientes(Clientes cliente)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var entity = await db.Clientes.FirstOrDefaultAsync(p => p.CodigoCliente == cliente.CodigoCliente);
                    bool cambioAsesor = false;
                    string asesorPrevio = string.Empty;

                    if (entity == null)
                    {
                        cliente.Empresa = await db.Empresa.FirstOrDefaultAsync(p => p.EmpresaId == cliente.EmpresaId);
                        cliente.MaestroMoneda = await db.MaestroMoneda.FirstOrDefaultAsync(p => p.IdMoneda == cliente.IdMoneda);
                        cliente.MaestroGrupoPrecio = await db.MaestroGrupoPrecio.FirstOrDefaultAsync(p => p.GrupoPrecio == cliente.GrupoPrecio);
                        db.Clientes.Add(cliente);
                    }
                    else
                    {
                        cambioAsesor = entity.CodigoAsesor != cliente.CodigoAsesor;
                        asesorPrevio = entity.CodigoAsesor;
                        entity.CodigoCliente = cliente.CodigoCliente;
                        cliente.Empresa = await db.Empresa.FirstOrDefaultAsync(p => p.EmpresaId == cliente.EmpresaId);
                        cliente.MaestroMoneda = await db.MaestroMoneda.FirstOrDefaultAsync(p => p.IdMoneda == cliente.IdMoneda);
                        cliente.MaestroGrupoPrecio = await db.MaestroGrupoPrecio.FirstOrDefaultAsync(p => p.GrupoPrecio == cliente.GrupoPrecio);
                        entity.Nombre = cliente.Nombre;
                        entity.Zona = cliente.Zona;
                        entity.ComunidadAutonoma = cliente.ComunidadAutonoma;
                        entity.GrupoCliente = cliente.GrupoCliente;
                        entity.Descuento = cliente.Descuento;
                        entity.Direccion = cliente.Direccion;
                        entity.FacturacionEntrega = cliente.FacturacionEntrega;
                        entity.Provincias = cliente.Provincias;
                        entity.Region = cliente.Region;
                        entity.Revision = cliente.Revision;
                        entity.LimiteCredito = cliente.LimiteCredito;
                        entity.CreditoDisponible = cliente.CreditoDisponible;
                        entity.ModoEntrega = cliente.ModoEntrega;
                        entity.Telefono = cliente.Telefono;
                        entity.GrupoImpuesto = cliente.GrupoImpuesto;
                        entity.CodigoAsesor = cliente.CodigoAsesor;
                        entity.Habilitado = cliente.Habilitado;
                        entity.IgnorarSeqFact = cliente.IgnorarSeqFact;
                        entity.IncluyeImpuesto = cliente.IncluyeImpuesto;
                        entity.Departamento = cliente.Departamento;
                        entity.Municipio = cliente.Municipio;
                        entity.Alias = cliente.Alias;
                        entity.DiasTransporte = cliente.DiasTransporte;
                    }
                    await db.SaveChangesAsync();

                    if (cambioAsesor)
                    {
                        db.SP_TrasladoPedidos(entity.CodigoCliente, cliente.CodigoAsesor);
                        db.SP_TrasladoRecibos(entity.CodigoCliente, cliente.CodigoAsesor);
                    }
                }
            }
            catch (Exception e)
            {
                
            }

            
        }


        public async Task SincronizarDirecciones(string empresa)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var clientes = db.Clientes.Where(p => p.EmpresaId.ToUpper() == empresa.ToUpper() && p.Habilitado).ToList();
                    foreach (var cliente in clientes)
                    {
                        var direcciones = new List<DireccionesCRMApiModel>();
                        var restClient = new RestClient(Enviroment.CRMWebServiceURLApi);
                        var request = new RestRequest($"clientes/direcciones/{cliente.CodigoCliente}", Method.GET);
                        request.Timeout = 5 * 60000;
                        request.AddHeader("Accept", "application/json");
                        IRestResponse response = restClient.Execute(request);

                        if (response.IsSuccessful && response.Content != "null")
                        {
                            direcciones = JsonConvert.DeserializeObject<List<DireccionesCRMApiModel>>(response.Content);
                            if (direcciones.Count > 0)
                            {
                                db.SPDesactivarDireccionesCliente(cliente.CodigoCliente);
                            }
                            foreach (var direccion in direcciones)
                            {
                                DireccionesxClienteViewModel reservadoClienteModel = new DireccionesxClienteViewModel
                                {
                                    codigoCliente = cliente.CodigoCliente,
                                    nombreDireccion = direccion.LOCATIONNAME,
                                    direccion = direccion.ADDRESS,
                                    postalAddress = direccion.POSTALADDRESS,
                                    principal = direccion.ISPRIMARY == 1,
                                };
                                await SyncDireccionCliente(reservadoClienteModel);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

        }

        private async Task SyncDireccionCliente(DireccionesxClienteViewModel direccion)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {

                    var entityFound = db.DireccionesCliente.FirstOrDefault(p => p.postalAddress == direccion.postalAddress && p.codigoCliente.ToUpper() == direccion.codigoCliente.ToUpper());

                    if (entityFound == null)
                    {
                        var newEntity = new DireccionesCliente
                        {
                            codigoCliente = direccion.codigoCliente,
                            postalAddress = direccion.postalAddress,
                            activo = true,
                            principal = direccion.principal,
                            fechaCreacion = DateTime.Now,
                            direccion = direccion.direccion,
                            nombreDireccion = direccion.nombreDireccion,
                        };

                        db.DireccionesCliente.Add(newEntity);
                    }
                    else
                    {
                        entityFound.activo = true;
                        entityFound.principal = direccion.principal;
                        entityFound.direccion = direccion.direccion;
                        entityFound.nombreDireccion = direccion.nombreDireccion;

                        db.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {

            }
        }

        private async Task SincronizarMaestroGrupoPrecio(MaestroGrupoPrecio maestroGrupoPrecio)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    var entity = await db.MaestroGrupoPrecio.FirstOrDefaultAsync(p => p.EmpresaId == maestroGrupoPrecio.EmpresaId && p.GrupoPrecio == maestroGrupoPrecio.GrupoPrecio);

                    if (entity == null)
                    {
                        db.MaestroGrupoPrecio.Add(maestroGrupoPrecio);
                    }
                    else
                    {
                        entity.Descripcion = maestroGrupoPrecio.Descripcion;
                    }
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                
            }                     
        }

        private int LimpiarClientes(string codigoAsesor, string empresa )
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    return  db.SP_Clientes_UpdateHabilitado(codigoAsesor, empresa);
                }
            }
            catch (Exception e)
            {
                return 0;
            }           
        }

        private async Task<List<Asesores>> VerificarAsesor(string codigoAsesor)
        {
            try
            {
                using (AVentasEntities db = new AVentasEntities())
                {
                    return await db.Asesores.AsNoTracking().Where(x => x.CodigoAsesor == codigoAsesor && x.Activo == true).ToListAsync();
                }
            }
            catch (Exception e)
            {
                return new List<Asesores>();
            }
        }
    }
}