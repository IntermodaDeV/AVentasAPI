using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AventasApi.enums
{
    public enum EstadoBodega 
    {
        Rechazado,
        Recepcionado,
        Transferido_a_bodega,       
        Pendiente_de_Aprobación_Ventas,
        Aprobadas,
        Todos
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