using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class DescuentoEnAcuerdosViewModel
    {
        public string CodigoDescuento { get; set; }

        public decimal Porcentaje { get; set; }

        public int Dias { get; set; }

        public string Empresa { get; set; }
    }
}