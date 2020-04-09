using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager
{
    public class ColoresXProductoManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerColores(string CodigoColeccion)
        {
            GestorColoresXProducto gestorColores = new GestorColoresXProducto();

            var coloresxProducto = gestorColores.ObtenerColoresDesdeCRMAPI(CodigoColeccion ?? " ").Result;
            if (LogicValidation.ValidateDataCount(coloresxProducto.Count))
            {
                var colores = coloresxProducto.Select(atr => atr.CreandoColor()).ToList();
                ColoresXProductoRepository colorRepository = new ColoresXProductoRepository();
                await colorRepository.SendToDatabase(colores, CodigoColeccion);
            }
        }
    }
}
