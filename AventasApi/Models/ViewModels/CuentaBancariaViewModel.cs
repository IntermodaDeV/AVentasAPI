using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class CuentaBancariaViewModel
    {
        public int IdCuentaBancaria { get; set; }
        public string NombreBanco { get; set; }
        public string NumeroCuenta { get; set; }
        public string Descripcion { get; set; }
        public string GrupoBanco { get; set; }
        public Nullable<int> IdBanco { get; set; }
        public string IdMoneda { get; set; }
        public string EmpresaId { get; set; }
    }
}