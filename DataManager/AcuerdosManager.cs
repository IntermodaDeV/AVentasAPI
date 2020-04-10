using DBData.Database;
using ExternalApiData.GestorData;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataManager.Extensions;
using DBData.Repositories;
namespace DataManager
{
   public class AcuerdosManager
    {
        public async Task<List<AcuerdosxCliente>> ObtnerAcuerdos()
        {
            GestorAcuerdosVenta gestorAcuerdosVenta = new GestorAcuerdosVenta();
            var acuerdos = gestorAcuerdosVenta.ObtenerAcuerdosXAsesor("", "").Result;
            if (acuerdos != null && acuerdos.Count > 0)
            {
                return acuerdos.Select(acu => acu.ToAcuerdoxCliente()).ToList();
            }
            return null;
        }
        public static async Task GuardarAcuerdos(List<AcuerdosxCliente> acuerdosAGuardar)
        {
            AcuerdosxClienteRepository acuerdosxClienteRepository  = new AcuerdosxClienteRepository();
              await acuerdosxClienteRepository.ModificarOAgregarAcuerdos(acuerdosAGuardar,"");
        }
    }
}
