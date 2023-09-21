using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AventasApi.Models.ViewModels
{
    public class ChequesContabilizadosViewModel
    {
        public string NumeroRecibo { get; set; }
        public System.DateTime FechaRecepcion { get; set; }
        public string NumeroCheque { get; set; }
        public string Banco { get; set; }
        public decimal ValorCheque { get; set; }
        public System.DateTime FechaVencimiento { get; set; }
        public string TipoCheque { get; set; }
        public string CodigoCliente { get; set; }
        public bool Activo { get; set; }


    }
}