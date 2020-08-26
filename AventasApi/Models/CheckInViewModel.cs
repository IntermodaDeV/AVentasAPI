using AventasApi.Models.ViewModels;
using System;

namespace AventasApi.Models
{
    public class CheckInViewModel
    {
        public int IdAsignacionxAsesor { get; set; }
        public Location location = new Location();
        public DateTime Fecha { get; set; }
    }
}