using System;
using System.Collections.Generic;

namespace AventasApi.Models.ViewModels
{
    public class PermisosViewModel
    {
        public int Id { get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public bool status { get; set; }
        public string EmpresaId { get; set; }

        public Nullable<bool> BloqueoCredito { get; set; }
        
        public List<RolesUsuariosViewModel> RolesUsuarios;
        public List<UsuariosEmpresasViewModel> EmpresasUsuarios;

        public PermisosViewModel()
        {
            this.RolesUsuarios = new List<RolesUsuariosViewModel>();
            this.EmpresasUsuarios = new List<UsuariosEmpresasViewModel>();
        }
    }
}