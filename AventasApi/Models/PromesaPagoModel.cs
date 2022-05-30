using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models
{
    public class PromesaPagoModel
    {
        public int IdPromesaPago { get; set; }
        public int IdAsignacionXAsesor { get; set; }
        public System.DateTime FechaPromesa { get; set; }
        public decimal Valor { get; set; }
        public System.DateTime FechaCrea { get; set; }
        public int UsuarioCrea { get; set; }
    }
}