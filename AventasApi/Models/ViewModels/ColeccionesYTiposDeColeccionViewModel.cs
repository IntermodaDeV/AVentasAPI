using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ColeccionesYTiposDeColeccionViewModel
    {
        public List<Colecciones> Colecciones = new List<Colecciones>();
        public List<TiposdeColeccion> TiposdeColeccion= new List<TiposdeColeccion>();
    }
}