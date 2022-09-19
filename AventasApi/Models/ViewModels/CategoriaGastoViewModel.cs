using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class CategoriaGastoViewModel
    {
        public int idCategoriaTipoGasto { get; set; }
        public int idTipoGastoViaje { get; set; }
        public string Nombre { get; set; }
        public string ProveedorPredefinido { get; set; }
        public string CuentaContrapartida { get; set; }
        public bool FacturaObligatoria { get; set; }
        public bool Descripcion { get; set; }
        public bool ImagenObligatoria { get; set; }

    }
}