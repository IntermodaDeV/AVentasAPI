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

    public static class FacturasExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static FacturasxCliente ToFacturasxCliente(this FacturasXClienteApiModel facturaPrev, List<TiposdePedido> tiposdePedidoList)
        {
            var tipodePedido = tiposdePedidoList.FirstOrDefault(tp => tp.TipoPedido == facturaPrev.DOC_TYPE);
            if (tipodePedido == null)
            {
                return null;
            }

            FacturasxCliente facturaNext = new FacturasxCliente
            {
                EmpresaId = facturaPrev.ENTITY,
                CodigoCliente = facturaPrev.ACCOUNT_NUM,
                Factura = facturaPrev.INVOICE,
                Tipo = facturaPrev.TRANS_TYPE,
                FechaFactura = facturaPrev.DOCUMENT_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(facturaPrev.DOCUMENT_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                TotalFactura = facturaPrev.AMOUNT_CUR != null ? Convert.ToDecimal(facturaPrev.AMOUNT_CUR) : 0,
                Saldo = facturaPrev.REMAIN_AMOUNT_CUR != null ? Convert.ToDecimal(facturaPrev.REMAIN_AMOUNT_CUR) : 0,
                PendienteFactura = facturaPrev.AMOUNT_PENDING != null ? Convert.ToDecimal(facturaPrev.AMOUNT_PENDING) : 0,
                FechaVencimiento = facturaPrev.DUE_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(facturaPrev.DUE_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                FechaMaxDescuento = facturaPrev.DISC_DATE == "01/01/1900" ? null : (DateTime?)DateTime.ParseExact(facturaPrev.DISC_DATE, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Descuento = facturaPrev.DISCOUNT != null ? Convert.ToDecimal(facturaPrev.DISCOUNT) : 0,
                IdMoneda = facturaPrev.CURRENCY_CODE,
                FacturaStatus = facturaPrev.STATUS,
                NumeroPagos = facturaPrev.N_PAYMENTS != null ? Convert.ToInt32(facturaPrev.N_PAYMENTS) : 0,
                Referencia = facturaPrev.REF_TRANS,
                IdLinea = facturaPrev.PROD_LINE == "" ? null : facturaPrev.PROD_LINE,
                IdTipoPedido = tipodePedido?.IdTipoPedido,
                NumeroFEL = facturaPrev.FACTURACION_FEL
            };

            return facturaNext;
        }

    }

}
