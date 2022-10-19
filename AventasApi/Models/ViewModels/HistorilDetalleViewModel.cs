using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class HistorilDetalleViewModel
    {
        public int IdGastoViajeDetalle { get; set; }
        public string Tipo { get; set; }
        public string categoria { get; set; }
        public string UsuarioAsesor { get; set; }
        public string Proveedor { get; set; }
        public string NoFactura { get; set; }
        public string Descripcion { get; set; }
        public string DescripcionAdmin { get; set; }
        public double ValorFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Byte[] imagen { get; set; }
        public string Admin { get; set; }
        public string serie { get; set; }
    }
}