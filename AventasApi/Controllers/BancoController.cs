using AventasApi.Infrastructure;
using AventasApi.Models;
using AventasApi.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AventasApi.Controllers
{
    public class BancoController : ApiController
    {
        AVentasEntities context = new AVentasEntities();
        [HttpGet]
        public async Task<IHttpActionResult> GetBancos()
        {

            var bancos = context.Bancos.Select(banco => new BancoViewModel
            {
                IdBanco = banco.IdBanco,
                NombreBanco = banco.NombreBanco,
                Descripcion = banco.Descripcion,
                EmpresaId = banco.EmpresaId,
                CuentasBancarias = banco.CuentasBancarias.Select(cuentBanc => new CuentaBancariaViewModel
                {
                    IdCuentaBancaria = cuentBanc.IdCuentaBancaria,
                    NombreBanco = cuentBanc.NombreBanco,
                    NumeroCuenta = cuentBanc.NumeroCuenta,
                    Descripcion = cuentBanc.Descripcion,
                    GrupoBanco = cuentBanc.GrupoBanco,
                    IdBanco = cuentBanc.IdBanco,
                    IdMoneda = cuentBanc.IdMoneda,
                    EmpresaId = cuentBanc.EmpresaId,
                }).ToList(),
            }).ToList();
            return Ok(bancos);
        }
    }
}
