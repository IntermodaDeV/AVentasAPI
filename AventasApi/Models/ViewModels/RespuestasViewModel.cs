using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class RespuestasViewModel
    {
        public RespuestasViewModel()
        {
            RespuestasDetalle = new List<RespuestasDetalleViewModel>();
        }
        public int Id { get; set; }
        public string CodigoCliente { get; set; }
        public int? UsuarioId { get; set; }
        public int? EncuestaId { get; set; }
        public string Usuario { get; set; }
        public List<RespuestasDetalleViewModel> RespuestasDetalle { get; set; }
    }
}