using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{
    public static class RutasExtension
    {
        public static Rutas CreandoRuta(this RutasXAsesorApiModel rutaXAsesor)
        {
            Rutas nuevoAtributo = new Rutas()
            {
                CodigoRuta = rutaXAsesor.ENTITY + "-" + rutaXAsesor.CODE,
                EmpresaId = rutaXAsesor.ENTITY,
                Nombre = rutaXAsesor.Description
            };
            return nuevoAtributo;
        }
    }
}
