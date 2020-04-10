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
    class BancoManager
    {
        private static readonly LogicValidation LogicValidation = new LogicValidation();

        public async Task ObtenerBancos()
        {
            GestorBancos gestorBancos = new GestorBancos();

            var bancos = gestorBancos.ObtenerBancosDesdeCRMAPI().Result;
            if (LogicValidation.ValidateDataCount(bancos.Count))
            {
                var listaBancos = bancos.Select(ban => ban.CreandoBanco()).ToList();
                BancosRepository bancoRepository = new BancosRepository();
                await bancoRepository.SendToDatabase(listaBancos);
            }
        }
    }
}
