using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Repositories;
using ExternalApiData.GestorData;
using ExternalApiData.Models.ApiModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManager
{
    public class AtributosXProductoManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerAtributos(string CodigoProducto)
        {
            var atributosXProducto = new List<AtributosXProductoCRMApiModel>();
            GestorAtributosXProductos gestorAtributos = new GestorAtributosXProductos();

            atributosXProducto = gestorAtributos.ObtenerAtributosDesdeCRMAPI(CodigoProducto ?? " ").Result;
            if (LogicValidation.ValidateDataCount(atributosXProducto.Count))
            {
                var atributos = atributosXProducto.Select(atr => atr.CreandoAtributo()).ToList();
                AtributosXProductoRepository atributosRepository = new AtributosXProductoRepository();
                await atributosRepository.SendToDatabase(atributos,  CodigoProducto);
            }
        }
    }
}
