using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class TipoVisitaClienteViewModel
    {
        public int idTipoVisita { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public Nullable<bool> Estatus { get; set; }
    }
}