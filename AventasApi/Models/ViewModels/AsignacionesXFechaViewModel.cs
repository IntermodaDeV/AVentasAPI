using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class AsignacionesXFechaViewModel
    {
        public List<AsignacionXAsesorViewModel> asignaciones;
        public DateTime? fecha { get; set; }

        public AsignacionesXFechaViewModel()
        {
            asignaciones = new List<AsignacionXAsesorViewModel>();
        }
    }
}