using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class GastoNoSincronizadosViewModel
    {
        public int IdGastoViajeDetalle { get; set; }
        public string tipo { get; set; }
        public string categoria { get; set; }
        public string UsuarioAsesor { get; set; }
        public string NoFactura { get; set; }
        public string Descripcion { get; set; }
        public string MensajeAX { get; set; }
        public double? importeExento { get; set; }
        public double? importeGravado { get; set; }
        public double ValorFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string serie { get; set; }
    }
}