using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AventasApi.Models.ViewModels
{
    public class MailModulosViewModel
    {
        
        public string ModuloId { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }

    }
}


