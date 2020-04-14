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
    public class TallasxGrupoManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task<List<TallasXGrupo>> Obtener()
        {
            GestorTallaXGrupoTalla gestor = new GestorTallaXGrupoTalla();

            var lineas = gestor.Obtener().Result;
            return lineas.Select(lin => lin.ToGrupoTallasXGrupo()).ToList();

        }
        public async Task<List<TallasXGrupo>> Guardar(List<TallasXGrupo> lineas)
        {
            TallasxGrupoRepository repository = new TallasxGrupoRepository();
          return  repository.ModificarOAgregar(lineas).Result;
        }
        public async Task IniciarProceso()
        {
            var lineas = Obtener().Result; ;
            var a = Guardar(lineas).Result;
        }
    }
}
