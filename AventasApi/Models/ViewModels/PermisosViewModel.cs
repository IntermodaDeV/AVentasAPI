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
        public Nullable<bool> TodosAsesores { get; set; }
        public Nullable<bool> BloqueoCredito { get; set; }
        public Nullable<bool> UsuarioOficina { get; set; }
        public Nullable<bool> AdministradorProductos { get; set; }
        public Nullable<bool> BodegaEspecifico { get; set; }

        public List<RolesUsuariosViewModel> RolesUsuarios;
        public List<UsuariosEmpresasViewModel> EmpresasUsuarios;
        public List<AsesoresUsuarioViewModel> AsesoresUsuario;
        public PermisosViewModel()
        {
            this.RolesUsuarios = new List<RolesUsuariosViewModel>();
            this.EmpresasUsuarios = new List<UsuariosEmpresasViewModel>();
            this.AsesoresUsuario = new List<AsesoresUsuarioViewModel>();
        }
    }
}