using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AventasApi.Models.ViewModels
{
    public class MailServiciosViewModel
    {
        public string Modulo { get; set; }
        public string ServicioID { get; set; }
        public string Descripcion { get; set; }
        public string UsuarioCreacion { get; set; }
        public string Header { get; set; }
        public string ValidaType { get; set; }
        public string Consulta { get; set; }
        public string Footer { get; set; }
        public string Estado { get; set; }
        public string valida_empresaid { get; set; }
    }
}
