using AventasApi.Utils;
using DataManager.Extensions;
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
    public class CuentasBancariasManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerEmpresas()
        {
            GestorCuentasBancarias gestorCuentasBancarias = new GestorCuentasBancarias();

            var cuentas = gestorCuentasBancarias.ObtenerCuentasDesdeCRMAPI().Result;
            if (LogicValidation.ValidateDataCount(cuentas.Count))
            {
                var listaCuentas = cuentas.Select(acu => acu.CreandoCuentaBancaria()).ToList();
                CuentasBancariasRepository cuentasBancariasRepository = new CuentasBancariasRepository();
                await cuentasBancariasRepository.SendToDatabase(listaCuentas);
            }
        }
    }
}
