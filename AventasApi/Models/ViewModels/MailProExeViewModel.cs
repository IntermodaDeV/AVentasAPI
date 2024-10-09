using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AventasApi.Models.ViewModels
{
    public class MailProExeViewModel
    {
        public int Id { get; set; }
        public string ServicioID { get; set; }
        public DateTime ProximaEjecucion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public string IntervalType { get; set; }
        public int IntervalValue { get; set; }
    }
}
