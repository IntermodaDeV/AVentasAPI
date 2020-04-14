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

    public static class GrupoTallasExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static GrupoTalla ToGrupoTalla(this GrupoTallaApiModel grupoTalla)
        {

            GrupoTalla grupoTallaAGuardar = new GrupoTalla
            {
                CodigoGrupoTalla = grupoTalla.CODIGO,
                Descripcion = grupoTalla.DESCRIPTION
            };
            return grupoTallaAGuardar;
        }

    }

}
