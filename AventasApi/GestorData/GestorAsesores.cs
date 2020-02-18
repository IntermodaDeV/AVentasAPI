using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AventasApi.Infrastructure;
using AventasApi.Models;
using AventasApi.Models.ApiModels;

namespace AventasApi.GestorData
{
    public class GestorAsesores
    {
        private string UrlString = @"http://190.109.223.244:8083/api/asesor/AsesoresDisponibles";
        private HttpClient client = new ClienteHttp();
        public bool ColeccionesActualizadas = false;
        public bool ErrorAlActualizar = false;
        //private static AVentasEntities context = new AVentasEntities();


        public async Task<List<Asesores>> ObtenerAsesores()
        {
            List<AsesorApiModel> asesores = null;
            List<Asesores> asesoresAGuardar = new List<Asesores>();
            HttpResponseMessage response = await client.GetAsync(UrlString).ConfigureAwait(false);
            asesores = await response.Content.ReadAsAsync<List<AsesorApiModel>>();
            if (response.IsSuccessStatusCode)
            {
                if (asesores != null && asesores.Count > 0)
                {
                    asesoresAGuardar = asesores.Select(ase => new Asesores
                    {
                        CodigoAsesor = ase.CODE,
                        Nombre = ase.NAME,
                        EmpresaId = ase.ENTITY,
                        Usuario = ase.CODE,
                        Diario = ase.JOURNAL
                    }
                    ).ToList();
                }
            }
            else
            {
                //throw new Exception("Error en la Peticion");
            }

            return asesoresAGuardar;


        }
        public async Task GuardarAsesores(List<Asesores> asesoresAGuardar)
        {
            using (AVentasEntities context = new AVentasEntities())
            {
                context.Asesores.AddRange(asesoresAGuardar);
                await context.SaveChangesAsync();
            }
        }
    }

}
