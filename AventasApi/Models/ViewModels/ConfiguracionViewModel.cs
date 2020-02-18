using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ConfiguracionViewModel
    {
        public int IdConfiguracion { get; set; }
        public string CodigoConfiguracion { get; set; }
        public string NombreConfiguracion { get; set; }
        public string DescripcionConfiguracion { get; set; }
        public string Valor { get; set; }
        public Nullable<System.DateTime> FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public Nullable<System.DateTime> FechaModificacion { get; set; }
        public string UsuarioModificacion { get; set; }
        public Nullable<bool> Activo { get; set; }
    }
}