using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class PermisosViewModel
    {
        public int Id { get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public bool status { get; set; }
        public string EmpresaId { get; set; }
        
        public List<RolesUsuariosViewModel> RolesUsuarios;

        public PermisosViewModel()
        {
            this.RolesUsuarios = new List<RolesUsuariosViewModel>();
        }
    }
}