using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class LogProformaViewModel
    {
        public int Id { get; set; }

        public string numProforma { get; set; }

        public int ProformaId { get; set; }

        public string Usuario { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Latitude { get; set; }

        public decimal longitude { get; set; }

    }
}