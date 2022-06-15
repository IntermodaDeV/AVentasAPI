using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class BitacoraNoVentaEnVisitasViewModel
    {
        public int IdNoVentaEnVisita { get; set; }

        public int IdRazonNoVenta { get; set; }

        public int IdAsignacionXAsesor { get; set; }

        public string Comentarios { get; set; }

        public Nullable<System.DateTime> FechaCrea { get; set; }

        public Nullable<int> UsuarioCrea { get; set; }
    }
}