using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class TallaViewModel
    {
        public string Talla { get; set; }
        public string GrupoTallaId { get; set; }
        public decimal Orden { get; set; }
        public List<DistribucionXTallaViewModel> Distribucion;
        public TallaViewModel()
        {
            Distribucion = new List<DistribucionXTallaViewModel>();
        }

    }
}