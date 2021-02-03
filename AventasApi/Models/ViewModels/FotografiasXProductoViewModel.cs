using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class FotografiasXProductoViewModel
    {
        public int IdFotografia { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }



        public string FotografiaProducto { get; set; }
        public Nullable<int> IdProducto { get; set; }
        public string CodigoColor { get; set; }
        public Nullable<bool> Principal { get; set; }
        public string NombreFotografia { get; internal set; }
    }
}