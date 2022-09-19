using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class TipoGastoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Diario { get; set; }
        public string Empresa { get; set; }
        public bool Activo { get; set; }
       
    }
}