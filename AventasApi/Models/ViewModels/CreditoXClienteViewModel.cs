using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class CreditoXClienteViewModel
    {
        public string Tipo { get; set; }
        public Nullable<decimal> Valor { get; set; }
        public Nullable<decimal> Disponible { get; set; }
        public Nullable<decimal> SaldoTotal { get; set; }
        public Nullable<decimal> C15Dias { get; set; }
    }
}