using System.Collections.Generic;

namespace AventasApi.Models.ViewModels
{
    public class ReservadoClienteViewModel
    {
        public string Coleccion { get; set; }
        public int UnidadesPendientes { get; set; }
        public decimal MontoPendiente { get; set; }
    }

    public class ReservadoClientePorLineaViewModel
    {
        public string Linea { get; set; }
        public int UnidadesPendientes { get; set; }
        public decimal MontoPendiente { get; set; }

        public List<ReservadoClienteColeccionesLineas> ReservadoClienteColeccionesLineas { get; set; }

        public ReservadoClientePorLineaViewModel()
        {
            this.ReservadoClienteColeccionesLineas = new List<ReservadoClienteColeccionesLineas>();
        }
    }


    public class ReservadoClienteColeccionesLineas
    {
        public string Coleccion { get; set; }
        public int UnidadesPendientes { get; set; }
        public decimal MontoPendiente { get; set; }
        public string Linea { get; set; }

    }
}
