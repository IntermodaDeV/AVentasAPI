using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class LimiteCreditoXClienteViewModel
    {
        public int IdRegistro { get; set; }
        public string Descripcion { get; set; }
        public Nullable<decimal> Valor { get; set; }
        public string CodigoCliente { get; set; }
    }
}