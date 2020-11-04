using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class RolesFuncionesViewModel
    {
        public int Id { get; set; }
        public int IdRol { get; set; }
        public string Funcion { get; set; }
        public Nullable<bool> Status { get; set; }

        public List<PantallasFuncionesViewModel> PantallasFunciones;

        public RolesFuncionesViewModel()
        {
            this.PantallasFunciones = new List<PantallasFuncionesViewModel>();
        }
    }
}