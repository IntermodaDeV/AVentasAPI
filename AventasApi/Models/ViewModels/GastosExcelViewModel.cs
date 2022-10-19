using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class GastosExcelViewModel
    {
        public string Tipo { get; set; }
        public string categoria { get; set; }
        public string descripcion { get; set; }
        public DateTime fecha { get; set; }
        public Double valor { get; set; }
        public string nombre { get; set; }

    }
}