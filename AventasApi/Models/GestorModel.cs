using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class GestorModel
    {
        public int ID { get; set; }
        public string NOMBRE { get; set; }
        public string DESCRIPCION { get; set; }
        public Nullable<int> ID_GP { get; set; }
        public Nullable<bool> STATUS { get; set; }
        public System.Guid ROWID { get; set; }
        public Nullable<System.DateTime> ULT_SINCRONIZACION { get; set; }
        public int T_MIN_SINC { get; set; }
        public bool FORZAR { get; set; }
        public int NIVEL_PRIORIDAD { get; set; }
        public Nullable<int> GRUPO { get; set; }
        public Nullable<int> ORDEN { get; set; }
        public Nullable<int> ID_MODULO { get; set; }
    }
}