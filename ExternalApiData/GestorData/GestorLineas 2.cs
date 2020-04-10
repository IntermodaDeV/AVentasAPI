using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ExternalApiData.Models.ApiModels;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ViewModels;

namespace ExternalApiData.GestorData
{
    public class GestorLineas
    {
        private string UrlString = $"{Enviroment.CRMWebServiceURLApi}clientes/{{0}}/{{1}}";

        public async Task<ClientesYMaestroGrupoPrecioViewModel> ObtenerClientesConRutaYMaestroGrupoPrecio(List<Rutas> rutasAsesor)
        {
            return new ClientesYMaestroGrupoPrecioViewModel();
        }
    }
}