using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class CategoriaGastoDetalleViewModel
    {
        public int idCategoriaTipoGastoViaje { get; set; }
        public int IdTipoGastoViaje { get; set; }
        public string TipoNombre { get; set; }
        public string Empresa { get; set; }
        public string CategoriaNombre { get; set; }
        public string ProveedorPredefinido { get; set; }
        public string GrupoImpuesto { get; set; }
        public string CuentaContrapartida { get; set; }
        public bool FacturaObligatoria { get; set; }
        public bool Descripcion { get; set; }
        public bool imagen { get; set; }
        public bool activo { get; set; }
    }
}