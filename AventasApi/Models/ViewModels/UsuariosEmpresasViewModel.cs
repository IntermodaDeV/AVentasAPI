using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class UsuariosEmpresasViewModel
    {
        public int Id { get; set; }
        public string EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public bool Status { get; set; }
    }


    public class UsuariosEmpresasParamViewmodel
    {
        public string EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public string usuario { get; set; }
    }

}