using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExternalApiData.GestorData
{
    public class GestorMaestroRutas
    {
        private readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}asesor/{{0}}/{{1}}/rutas";

        public async Task<List<RutasXAsesorApiModel>> ObtenerAtributosDesdeCRMAPI(string CodigoProducto)
        {
            var rutas = new List<RutasXAsesorApiModel>();
            await Task.Run(() =>
            {
                string peticion = string.Format(UrlString, CodigoProducto);
                var restClient = new RestClient(peticion);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                IRestResponse response = restClient.Execute(request);

                if (response.IsSuccessful)
                {
                    rutas = JsonConvert.DeserializeObject<List<RutasXAsesorApiModel>>(response.Content);
                }
            });
            return rutas;
        }

        //public async Task<List<Rutas>> ObtenerRutas()
        //{
        //    List<Asesores> asesores = new List<Asesores>();
        //    List<Rutas> rutasXAgregar = new List<Rutas>();
        //    using (AVentasEntities context = new AVentasEntities())
        //    {
        //        asesores = context.Asesores.AsNoTracking().ToList();
        //    }
        //    Parallel.ForEach(asesores, ase =>
        //      {

        //          List<RutasXAsesorApiModel> rutas = new List<RutasXAsesorApiModel>();
        //          string peticion = string.Format(UrlString, ase.EmpresaId,ase.Diario);
        //          var restClient = new RestClient(peticion);
        //          var request = new RestRequest(Method.GET);
        //          request.AddHeader("Accept", "application/json");
        //          IRestResponse response = restClient.Execute(request);

        //          if (response.IsSuccessful)
        //          {
        //              rutas = JsonConvert.DeserializeObject<List<RutasXAsesorApiModel>>(response.Content);
        //              foreach (var rutaXAsesor in rutas)
        //              {
        //                  var rutaAAgregar = new Rutas
        //                  {
        //                      CodigoRuta = rutaXAsesor.ENTITY + "-" + rutaXAsesor.CODE,
        //                      EmpresaId = rutaXAsesor.ENTITY,
        //                      Nombre = rutaXAsesor.Description
        //                  };
        //                  lock (rutasXAgregar)
        //                  {
        //                      if (!rutasXAgregar.Any(rutXAgre => rutXAgre.CodigoRuta == rutaAAgregar.CodigoRuta))
        //                      {
        //                          rutasXAgregar.Add(rutaAAgregar);
        //                      }
        //                  }
        //              }
        //          }
        //      });
        //    return rutasXAgregar;


        //}
        //public async Task GuardarRutas(List<Rutas> rutasAGuardar)
        //{
        //    using (AVentasEntities context = new AVentasEntities())
        //    {
        //        context.Rutas.AddRange(rutasAGuardar.OrderBy(rut=>rut.EmpresaId).ThenBy(rut => rut.CodigoRuta));
        //        await context.SaveChangesAsync();
        //    }
        //}
    }
}