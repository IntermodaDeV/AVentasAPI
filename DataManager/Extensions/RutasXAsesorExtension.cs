using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{
    public static class RutasXAsesorExtension
    {
        public static RutasxAsesor CreandoRuta(this RutasXAsesorApiModel rutaXAsesor)
        {
            RutasxAsesor nuevaRuta = new RutasxAsesor()
            {
                CodigoRuta = rutaXAsesor.ENTITY.ToLower() + "-" + rutaXAsesor.CODE,
                CodigoAsesor = null,
            };
            return nuevaRuta;
        }
    }
}
