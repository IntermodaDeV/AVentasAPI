using System;

namespace AventasApi.Models
{
    public class ReciboAnuladoEmailModel
    {
        public string NumeroRecibo { get; set; }
        public string Cliente { get; set; }
        public string CodigoCliente { get; set; }
        public decimal ValorRecibo { get; set; }
        public string MonedaSimbolo { get; set; }
        public string MotivoAnulacion { get; set; }
        public Nullable<DateTime> FechaAnulado { get; set; }
        public string ComentarioAnulacion { get; set; }
        public string Asesor { get; set; }
        public string CodigoEmpresa { get; set; }
        public bool EsAnticipo { get; set; }
        public Nullable<DateTime> FechaRecibo { get; set; }
        public Nullable<DateTime> FechaCreacion { get; set; }
        public string TipoPagoDescripcion { get; set; }
        public string SpecPagoDescripcion { get; set; }
        public string UsuarioAnulo { get; set; }
    }
}
