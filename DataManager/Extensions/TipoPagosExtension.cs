using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{
    public static class TipoPagosExtension
    {
        public static TiposdePago CreandoTipoPago(this TiposPagoCRMApiModel tipo)
        {
            TiposdePago nuevoTipo = new TiposdePago()
            {
                Codigo = tipo.CODE,
                Descripcion = tipo.DESCRIPTION,
                Tipo = tipo.TYPE,
                EmpresaId = tipo.COMPANY_CODE,
            };
            return nuevoTipo;
        }
    }
}
