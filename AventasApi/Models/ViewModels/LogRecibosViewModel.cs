using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class LogRecibosViewModel
    {
        public int Id { get; set; }

        public string numRecibo { get; set; }

        public int ReciboId { get; set; }

        public string Usuario { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Latitude { get; set; }

        public decimal longitude { get; set; }
    }
}