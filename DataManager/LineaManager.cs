using AventasApi.Utils;
using DataManager.Extensions;
using DBData.Database;
using DBData.Repositories;
using ExternalApiData.GestorData;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager
{
    public class LineaManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task<List<MaestroLinea>> Obtener()
        {
            GestorLineas gestorLinea = new GestorLineas();

            var lineas = gestorLinea.ObtenerLineas().Result;
            return lineas.Select(lin => lin.ToMaestroLineas()).ToList();

        }
        public async Task<List<MaestroLinea>> Guardar(List<MaestroLinea> lineas)
        {
            LineasRepository repository = new LineasRepository();
          return  repository.ModificarOAgregarLineas(lineas).Result;
        }
        public async Task IniciarProceso()
        {
            var lineas = Obtener().Result; ;
            var a = Guardar(lineas).Result;
        }
    }
}
