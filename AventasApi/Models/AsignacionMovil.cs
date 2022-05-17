using System;

namespace AventasApi.Models
{
    public class AsignacionMovil
    {
        public int IdAsignacionxAsesor { get; set; }
        public bool BloqueoCheckin { get; set; }
        public bool BloqueoCheckout { get; set; }
        public bool Cancelada { get; set; }
        public bool Deshabilitada { get; set; }
        public string Observacion { get; set; }
        public DateTime? FechaCheckIn { get; set; }
        public DateTime? FechaCheckOut { get; set; }
        public DateTime? HoraInicio { get; set; }
        public DateTime? HoraFinal { get; set; }
        public int idPrioridad { get; set; }
        public int idRazonNoVentaTipo { get; set; }
        public int idRazonNoVentaCausa { get; set; }
        public decimal? LatitudeCheckIn { get; set; }
        public decimal? LongitudeCheckIn { get; set; }
        public decimal? LatitudeCheckOut { get; set; }
        public decimal? LongitudeCheckOut { get; set; }
        public DateTime? Fecha { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public string CodigoCliente { get; set; }
        public string CodigoAsesor { get; set; }
    }
}