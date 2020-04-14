using DBData.Database;
using ExternalApiData.Models;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{

    public static class LineasExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static MaestroLinea ToMaestroLineas(this LineaApiModel linea)
        {

            MaestroLinea lineaAGuardar = new MaestroLinea
            {
                IdLinea = linea.CODIGO,
                Linea = linea.DESCRIPTION
            };
            return lineaAGuardar;
        }

    }

}
