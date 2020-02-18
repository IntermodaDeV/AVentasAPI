using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class CuotasXAcuerdosViewModel
    {
        public string Acuerdo { get; set; }
        public List<CuotasViewModel> CuotasXAcuerdos;
        public CuotasXAcuerdosViewModel()
        {
            this.CuotasXAcuerdos = new List<CuotasViewModel>();

        }
    }
}