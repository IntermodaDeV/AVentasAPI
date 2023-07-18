using ExternalApiData.Enviroments;
using RestSharp;

namespace AventasApi.Utils
{
    public class APIPedidoStatus
    {
        public int APSALESTATUS { get; set; }
    }
    public class SyncPedido
    {
        public static int ObtenerEstadoPedido(string pedido, string empresa)
        {
            var restClient = new RestClient(Enviroment.CRMWebServiceURLApi);
            var request = new RestRequest($"api/pedidos/{pedido}/{empresa}/status", Method.GET);
            request.Timeout = 480 * (2000);
            request.AddHeader("Accept", "application/json");
            var response = restClient.Execute<APIPedidoStatus>(request);

            if (response.IsSuccessful)
            {
                var content = Newtonsoft.Json.JsonConvert.DeserializeObject<APIPedidoStatus>(response.Content);
                return content.APSALESTATUS;
            }

            return 0;
        }
    }
}
