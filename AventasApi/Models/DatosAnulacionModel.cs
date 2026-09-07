using System;

namespace AventasApi.Models
{
    // Datos de anulación en común entre RecibosxCliente y AnticiposxCliente,
    // usados para armar y enviar el correo de anulación.
    public class DatosAnulacionModel
    {
        public string NumeroRecibo { get; set; }
        public string CodigoCliente { get; set; }
        public string IdMoneda { get; set; }
        public string CodigoAsesor { get; set; }
        public Nullable<decimal> Valor { get; set; }
        public bool Anulado { get; set; }
        public Nullable<int> MotivoAnulacionId { get; set; }
        public Nullable<DateTime> FechaAnulacion { get; set; }
        public string ComentarioAnulacion { get; set; }
        public bool EsAnticipo { get; set; }
    }
}
