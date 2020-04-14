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
    public class GrupoTallasManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task<List<GrupoTalla>> Obtener()
        {
            GestorGrupoTalla gestorLinea = new GestorGrupoTalla();

            var lineas = gestorLinea.ObtenerGruposTalla().Result;
            return lineas.Select(lin => lin.ToGrupoTalla()).ToList();

        }
        public async Task<List<GrupoTalla>> Guardar(List<GrupoTalla> lineas)
        {
            GrupoTallasRepository repository = new GrupoTallasRepository();
          return  repository.ModificarOAgregar(lineas).Result;
        }
        public async Task IniciarProceso()
        {
            var lineas = Obtener().Result; ;
            var a = Guardar(lineas).Result;
        }
    }
}
