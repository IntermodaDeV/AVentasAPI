using AventasApi.GestorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AventasApi.Singleton
{
    sealed class GestorManager
    {
        private static readonly GestorManager _instance = new GestorManager();
        public static Task TaskActualizarFacturas = new Task(GestorFacturasXCliente.ActualizarFacturas);

        static GestorManager()
        {
        }
        private GestorManager()
        {
        }
        //public Task ActualizarFacturas()
        //{

        //}
        public static GestorManager Instance()
        {
            return _instance;
        }
    }
}