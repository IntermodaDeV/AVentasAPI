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

    public enum EstadoBodegaEmail
    {
        Sin_Recibir_en_calidad,
        Recepcionada_en_calidad,
        Transferida_a_bodega,
        Aprobada_en_calidad,
        Rechazada_en_calidad
    }
}