using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class RolesUsuariosViewModel
    {
        public int Id { get; set; }
        public bool status { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        
        public List<RolesFuncionesViewModel> RolesFunciones;
        public RolesUsuariosViewModel()
        {
            this.RolesFunciones = new List<RolesFuncionesViewModel>();
        }
    }
}