using DBData.Database;
using ExternalApiData.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManager.Extensions
{

    public static class AcuerdoCRMApiModelExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static AcuerdosxCliente ToAcuerdoxCliente(this AcuerdoCRMApiModel acu, int IdTipoPedido)
        {
            decimal total, saldo, liberado, entregado, facturado = 0;
            decimal.TryParse(acu.AMOUNT, out total);
            decimal.TryParse(acu.REMAINING, out saldo);
            decimal.TryParse(acu.RELEASED, out liberado);
            decimal.TryParse(acu.DELIVERED, out entregado);
            decimal.TryParse(acu.INVOICED, out facturado);
            AcuerdosxCliente acuerdo = new AcuerdosxCliente
            {
                IdAcuerdoxCliente = acu.ID_SALES_AGREEMENT,
                CodigoCliente = acu.CUSTOMER_ACCOUNT,
                IdTipoPedido = IdTipoPedido,
                IdMoneda = acu.CURRENCY,
                EmpresaId = acu.ENTITY,
                Total = total,
                Saldo = saldo,
                Liberado = liberado,
                Facturado = facturado,
                Entregado = entregado,
                //IdLinea = acu.,
            };
            return acuerdo;
        }
        public static AcuerdosxCliente ToAcuerdoxCliente(this AcuerdoCRMApiModel acu)
        {
            decimal total, saldo, liberado, entregado, facturado = 0;
            decimal.TryParse(acu.AMOUNT, out total);
            decimal.TryParse(acu.REMAINING, out saldo);
            decimal.TryParse(acu.RELEASED, out liberado);
            decimal.TryParse(acu.DELIVERED, out entregado);
            decimal.TryParse(acu.INVOICED, out facturado);
            AcuerdosxCliente acuerdo = new AcuerdosxCliente
            {
                IdAcuerdoxCliente = acu.ID_SALES_AGREEMENT,
                CodigoCliente = acu.CUSTOMER_ACCOUNT,
                IdMoneda = acu.CURRENCY,
                EmpresaId = acu.ENTITY,
                Total = total,
                Saldo = saldo,
                Liberado = liberado,
                Facturado = facturado,
                Entregado = entregado,
                //IdLinea = acu.,
            };
            return acuerdo;
        }
    }

}
