using System;
using System.Collections.Generic;
using System.Linq;

namespace Proxy
{
    public class Proxy
    {
        public static List<IMEmpresasTransporte_Result> GetEmpresasTransporte(string empresa)
        {
            try
            {
                using (var context = new AxProduccionEntities())
                {
                    return context.IMEmpresasTransporte(empresa).ToList();
                }
            }
            catch (Exception e)
            {
                return new List<IMEmpresasTransporte_Result>();
            }
        }

        public static List<IMObtenerPrecioCaja_Result> GetTransportePrecioCaja(string empresa)
        {
            try
            {
                using (var context = new AxProduccionEntities())
                {
                    return context.IMObtenerPrecioCaja(empresa).ToList();
                }
            }
            catch (Exception e)
            {
                return new List<IMObtenerPrecioCaja_Result>();
            }
        }

        public static List<IMObtenerComunidadAutonoma_Result> GetComunidadAutonoma(string Pais)
        {
            try
            {
                using (var context = new AxProduccionEntities())
                {
                    return context.IMObtenerComunidadAutonoma(Pais).ToList();
                }
            }
            catch (Exception e)
            {
                return new List<IMObtenerComunidadAutonoma_Result>();
            }
        }
    }
}
