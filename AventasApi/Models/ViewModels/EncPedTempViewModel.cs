using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class EncPedTempViewModel
    {
        public string CodigoCliente { get; set; }
        public string CodigoColeccion { get; set; }
        public string TipoColeccion { get; set; }
        public string IdLinea{ get; set; }
        public string AcuerdoVenta { get; set; }
        public bool Crear{ get; set; }
    }
}