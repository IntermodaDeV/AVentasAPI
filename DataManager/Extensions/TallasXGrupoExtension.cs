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

    public static class TallasXGrupoExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static TallasXGrupo  ToGrupoTallasXGrupo(this TallaPorGrupoTalla grupoTalla)
        {

            TallasXGrupo grupoTallaAGuardar = new TallasXGrupo
            {

                CodigoTalla = grupoTalla.SIZE,
                CodigoGrupoTalla = grupoTalla.SIZE_CHART,
                Orden = (int)float.Parse(grupoTalla.ORDEN)
            };
            return grupoTallaAGuardar;
        }

    }

}
