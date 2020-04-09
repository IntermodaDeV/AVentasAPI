using DBData.Database;
using DBData.Repositories;
using ExternalApiData.GestorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager.Extensions;

namespace DataManager
{
    class ColeccionesManager
    {
        public async Task<List<Colecciones>> ObtnerColecciones()
        {
            GestorColecciones gestorColecciones = new GestorColecciones();
            var colecciones = gestorColecciones.ObtenerColecciones().Result;
            if (colecciones != null && colecciones.Count > 0)
            {
                return colecciones.Select(col => col.ToColecciones()).ToList();
            }
            return null;
        }
        public  async Task<List<Colecciones>> GuardarColecciones(List<Colecciones> coleccoinesAGuardar)
        {
            ColeccionesRepository acuerdosxClienteRepository = new ColeccionesRepository();
            return acuerdosxClienteRepository.ModificarOAgregarColecciones(coleccoinesAGuardar).Result;
        }
        public  async Task IniciarProceso(List<Colecciones> coleccoinesAGuardar)
        {
           var colecciones = ObtnerColecciones().Result; ;
            var a = GuardarColecciones(colecciones).Result;
        }
    }
}
