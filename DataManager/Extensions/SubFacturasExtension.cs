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

    public static class SubFacturasExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static SubFacturasxCliente ToSubFacturasxCliente(this SubFacturasXClienteApiModel subFacturaPrev, List<FacturasxCliente> facturasList, List<AcuerdosxCliente> acuerdosList)
        {
            var factura = facturasList.FirstOrDefault(fac => fac.Referencia == subFacturaPrev.REF_CUSTTRANS);
            if (factura == null)
            {
                return new SubFacturasxCliente();
            }

            SubFacturasxCliente subFacturaNext = new SubFacturasxCliente
            {
                EmpresaId = subFacturaPrev.ENTITY,
                CodigoCliente = subFacturaPrev.ACCOUNT_NUM,
                Factura = subFacturaPrev.INVOICE,
                Saldo = subFacturaPrev.AMOUNT_MST != null ? Convert.ToDecimal(subFacturaPrev.AMOUNT_MST) : 0,
                SaldoDivisa = subFacturaPrev.AMOUNT_CUR != null ? Convert.ToDecimal(subFacturaPrev.AMOUNT_CUR) : 0,
                FechaVencimiento = subFacturaPrev.DUE_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(subFacturaPrev.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                FechaMaxDescuento = subFacturaPrev.LIMIT_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(subFacturaPrev.LIMIT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                FechaVencimientoDescuento = subFacturaPrev.DISC_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(subFacturaPrev.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Descuento = subFacturaPrev.DISC_AMOUNT != null ? Convert.ToDecimal(subFacturaPrev.DISC_AMOUNT) : 0,
                PendientePago = subFacturaPrev.PAYM_AMOUNT != null ? Convert.ToDecimal(subFacturaPrev.PAYM_AMOUNT) : 0,
                Referencia = subFacturaPrev.REF_TRANSOPEN,
                ReferenciaFacturas = subFacturaPrev.REF_CUSTTRANS,
                ReferenciaAcuerdo = subFacturaPrev.AGREEMENT_NUM,
                NumeroCuota = subFacturaPrev.PA_PAYM_NUM != null ? Convert.ToInt32(subFacturaPrev.PA_PAYM_NUM) : 0,
                ValorCuota = subFacturaPrev.PA_PAYM_AMOUNT != null ? Convert.ToDecimal(subFacturaPrev.PA_PAYM_AMOUNT) : 0,
                ValorVencidoCuota = subFacturaPrev.PA_DUE_AMOUNT != null ? Convert.ToDecimal(subFacturaPrev.PA_DUE_AMOUNT) : 0,
                ReferenciaCuotas = subFacturaPrev.PA_REF_APSA,
                IdMoneda = subFacturaPrev.CURRENCY_CODE,
                IdAcuerdoxCliente = subFacturaPrev.AGREEMENT_NAME == "" ? null : subFacturaPrev.AGREEMENT_NAME,
                IdFactura = factura.IdFactura
            };

            return subFacturaNext;
        }

    }

}
