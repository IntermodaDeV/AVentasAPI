using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class MotivosDevolucionViewModel
    {
        public int IdMotivoDevolucion { get; set; }

        public string CodigoMotivoDevolucion { get; set; }

        public string Descripcion { get; set; }

        public string EmpresaId { get; set; }

        public bool aprobacionObligatoria { get; set; }

        public bool Estado { get; set; }

        public Nullable<System.DateTime> FechaModifica { get; set; }

        public Nullable<int> UsuarioModifica { get; set; }
    }
}