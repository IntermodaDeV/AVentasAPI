using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.enums
{
    public enum EstadoBodega 
    {
        Sin_Recibir_en_calidad,
        Recepcionadas_en_calidad,
        Transferidas_a_bodega,
        Aprobadas_en_calidad,
        Rechazadas_en_calidad
    }
}