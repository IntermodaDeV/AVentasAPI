using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{
    public static class AsesoresExtension
    {
        public static Asesores CreandoAsesor(this AsesorApiModel asesor)
        {
            Asesores nuevoAsesor = new Asesores()
            {
                CodigoAsesor = asesor.CODE,
                Nombre = asesor.NAME,
                EmpresaId = asesor.ENTITY,
                Usuario = asesor.CODE,
                Diario = asesor.JOURNAL
            };
            return nuevoAsesor;
        }
    }
}
