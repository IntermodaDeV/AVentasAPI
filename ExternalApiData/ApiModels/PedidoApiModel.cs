using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
    public class SalesTable
    {
        public string AcuerdoVentaId { get; set; }
        public string Cliente { get; set; }
        public string Linea { get; set; }
        public LineaOrden lineaOrden { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string CodigoColeccion { get; set; }
        public string Orden { get; set; }
        public string TipoColeccion { get; set; }
        public string TipoVenta { get; set; }

    }
    public class LineaOrden
    {
        public List<LineaArticulo> parmSalesLineList { get; set; }

        public LineaOrden()
        {
            parmSalesLineList= new List<LineaArticulo>();
        }
    }

    public class LineaArticulo
    {
        public string Articulo { get; set; }
        public decimal Cantidad { get; set; }
        public string Color { get; set; }
        public int Linea { get; set; }
        public decimal MontoLinea { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Talla { get; set; }
        public bool Eliminar { get; set; }
        public bool Actualizar { get; set; }
        
    }
    public class PedidoApiModel
    {
        public string userName { get; set; }
        public string password { get; set; }
        public SalesTable SalesTable { get; set; }
        public bool Crear { get; set; }
    }
}