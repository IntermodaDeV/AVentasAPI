using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class PosteoColeccionSincEspecificoModel
    {
        public int IdGestor { get; set; }
        public string EmpresaId { get; set; }
        public string ColeccionId { get; set; }
        public string Usuario { get; set; }
    }

    public class PosteoCancelarListaEspecificoModel
    {
        public int IdLista { get; set; }
    }
}