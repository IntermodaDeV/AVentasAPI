using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class GoeposicionXAsesorViewModel
    {
        public int IdBitacoraGeo { get; set; }
        public Nullable<int> IdAsignacionxAsesor { get; set; }
        public Nullable<bool> Mocked { get; set; }
        public Nullable<decimal> Accuracy { get; set; }
        public Nullable<decimal> Altitude { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public string CodigoAsesor { get; set; }
    
        
    }
}