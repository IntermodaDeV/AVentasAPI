using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class ListaEjecucionManualModel
    {
        public int ID { get; set; }
        public Nullable<int> ID_GESTOR { get; set; }
        public bool EN_ESPERA { get; set; }
        public bool EN_EJECUCION { get; set; }
        public bool FINALIZADO { get; set; }
        public string USUARIO { get; set; }
        public Nullable<System.DateTime> FECHA { get; set; }
    }

    public class ListaEjecucionManuaVisuallModel
    {
        public int ID { get; set; }
        public Nullable<int> ID_GESTOR { get; set; }
        public string NOMBRE { get; set; }
        public bool EN_ESPERA { get; set; }
        public bool EN_EJECUCION { get; set; }
        public bool FINALIZADO { get; set; }
        public string USUARIO { get; set; }
        public string FECHASTR { get; set; }
        public DateTime? FECHA { get; set; }
        public int ID_MODULO { get; set; }
        public string MODULO { get; set; }
    }
}