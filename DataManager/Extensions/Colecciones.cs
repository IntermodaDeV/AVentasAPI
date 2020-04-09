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

    public static class ColeccionCRMApiModelExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static Colecciones ToColecciones(this ColeccionCRMApiModel coleccion)
        {

            Colecciones coleccionAGuardar = new Colecciones
            {
                CodigoColeccion = coleccion.PACKAGE,
                Nombre = coleccion.NAME,
                ColeccionTipo = coleccion.PACKAGE_TYPE,
                EmpresaId = coleccion.ENTITY,
                DisenoInicio = DateTime.ParseExact(coleccion.START_DATE_DESIGN, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                DisenoFinal = DateTime.ParseExact(coleccion.END_DATE_DESIGN, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                EntregaInicio = DateTime.ParseExact(coleccion.START_DATE_DELIVERY_SALES_ORDER, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                EntregaFinal = DateTime.ParseExact(coleccion.END_DATE_DELIVERY_SALES_ORDER, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                ProduccionInicio = DateTime.ParseExact(coleccion.START_DATE_PRODUCTION, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                ProduccionFinal = DateTime.ParseExact(coleccion.END_DATE_PRODUCTION, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                VentaInicio = DateTime.ParseExact(coleccion.START_DATE_SALES_ORDER_ENTRY, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                VentaFinal = DateTime.ParseExact(coleccion.END_DATE_SALES_ORDER_ENTRY, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                Estatus = int.Parse(coleccion.STATUS),
                FotoPortada = @"https://aventas.devcit.com:3044/ImagenesXProducto/portadacoleccion.jpg",
            };
            return coleccionAGuardar;
        }

    }

}
