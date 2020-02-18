using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class BancoViewModel
    {
        public int IdBanco { get; set; }
        public string NombreBanco { get; set; }
        public string Descripcion { get; set; }
        public string EmpresaId { get; set; }
        public virtual ICollection<CuentaBancariaViewModel> CuentasBancarias { get; set; }
        public BancoViewModel()
        {
            this.CuentasBancarias = new HashSet<CuentaBancariaViewModel>();
        }
    }
}