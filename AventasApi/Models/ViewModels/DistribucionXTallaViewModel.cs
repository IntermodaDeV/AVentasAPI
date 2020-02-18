using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class DistribucionXTallaViewModel
    {

        public int IdDistribucion { get; set; }
        public Nullable<int> IdTallaxGrupo { get; set; }
        public string NombreDistribucion { get; set; }
        public string NombreTalla { get; set; }
        public string Cantidad { get; set; }
    }
}