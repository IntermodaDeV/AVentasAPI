using AventasApi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.Models.ViewModels
{
    public class ClientesYMaestroGrupoPrecioViewModel
    {
        public List<Clientes> ClientesConRuta;
        public List<MaestroGrupoPrecio> MaestroGrupoPrecio;
        public  List<Rutas> Rutas = new List<Rutas>();
        public ClientesYMaestroGrupoPrecioViewModel()
        {
            MaestroGrupoPrecio = new List<MaestroGrupoPrecio>();
            ClientesConRuta = new List<Clientes>();
        }
    }
}