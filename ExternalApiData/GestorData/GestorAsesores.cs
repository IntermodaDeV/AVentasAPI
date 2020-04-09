using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExternalApiData.Enviroments;
using ExternalApiData.Models;
using ExternalApiData.Models.ApiModels;
using RestSharp;
using Newtonsoft.Json;
using AventasApi.Utils;
using DBData.Database;

namespace ExternalApiData.GestorData
{
    public class GestorAsesores
    {
        private readonly string UrlString = $"{Enviroment.CRMWebServiceURLApi}asesor/AsesoresDisponibles";
        private LogicValidation LogicValidation = new LogicValidation();


        public async Task<List<AsesorApiModel>> ObtenerAsesores()
        {
            var restClient = new RestClient(UrlString);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            IRestResponse response = restClient.Execute(request);
            if (response.IsSuccessful)
            {
                List<AsesorApiModel> asesores = JsonConvert.DeserializeObject<List<AsesorApiModel>>(response.Content);

                if (LogicValidation.ValidateDataCount(asesores.Count))
                {
                    asesores = asesores.Where(ase => LogicValidation.IsDataValid(ase))
                    .ToList();
                    //await GuardarAsesores(asesoresAGuardar);
                }
                return asesores;
            }
            return new List<AsesorApiModel>();
        }

        //public static async Task GuardarAsesores(List<Asesores> ListaAsesores)
        //{
        //    int updateCount = 0, insertCount = 0, errorCount = 0;
        //    foreach (var asesor in ListaAsesores)
        //    {
        //        if (LogicValidation.IsDataValid(asesor))
        //        {
        //            using (AVentasEntities context = new AVentasEntities())
        //            {
        //                var asesorBD = await context.Asesores.FirstOrDefaultAsync(x => x.CodigoAsesor == asesor.CodigoAsesor
        //                                && x.EmpresaId == asesor.EmpresaId);
        //                if (LogicValidation.IsDataValid(asesorBD))
        //                {
        //                    var asesorModel = new AsesoresAPIViewModel(asesorBD);
        //                    var asesorAPI = new AsesoresAPIViewModel(asesor);

        //                    bool resul = EvaluarModelos(asesorModel, asesorAPI);
        //                    if (!resul)
        //                    {
        //                        updateCount++;
        //                        context.Entry(asesorBD).State = EntityState.Modified;
        //                        asesorBD.CodigoAsesor = asesor.CodigoAsesor;
        //                        asesorBD.Nombre = asesor.Nombre;
        //                        asesorBD.EmpresaId = asesor.EmpresaId;
        //                        asesorBD.Usuario = asesor.Usuario;
        //                        asesorBD.Diario = asesor.Diario;

        //                        try
        //                        {
        //                            context.SaveChanges();
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            errorCount++;
        //                            Console.WriteLine(ex);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    insertCount++;
        //                    errorCount += CreandoAsesor(asesor);
        //                }
        //            }
        //        }
        //    }
        //    string counter = updateCount.ToString() + "-" + insertCount.ToString() + "-" + errorCount.ToString();
        //    LogicValidation.EmailNotification("GestorAsesores", counter);
        //}

        public static bool EvaluarModelos(object imagenBD, object imagen)
        {
            if (imagenBD == null || imagen == null)
            {
                return false;
            }

            if (imagenBD.GetType() != imagen.GetType())
            {
                return false;
            }

            var Props = imagenBD.GetType().GetProperties();
            foreach (var Prop in Props)
            {
                var aPropValue = Prop.GetValue(imagenBD) ?? string.Empty;
                var bPropValue = Prop.GetValue(imagen) ?? string.Empty;
                if (aPropValue.ToString() != bPropValue.ToString())
                    return false;
            }
            return true;
        }

        //public static int CreandoAsesor(Asesores asesor)
        //{
        //    int contador = 0;
        //    using (AVentasEntities context = new AVentasEntities())
        //    {
        //        Asesores nuevoBanco = new Asesores()
        //        {
        //            CodigoAsesor = asesor.CodigoAsesor,
        //            Nombre = asesor.Nombre,
        //            EmpresaId = asesor.EmpresaId,
        //            Usuario = asesor.Usuario,
        //            Diario = asesor.Diario,
        //        };
        //        context.Asesores.Add(nuevoBanco);

        //        try
        //        {
        //            context.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            contador++;
        //            Console.WriteLine(ex);
        //        }
        //    }
        //    return contador;
        //}

        public class AsesoresAPIViewModel
        {
            public string CodigoAsesor { get; set; }
            public string Nombre { get; set; }
            public string EmpresaId { get; set; }
            public string Usuario { get; set; }
            public string Diario { get; set; }

            public AsesoresAPIViewModel(Asesores asesor)
            {
                CodigoAsesor = asesor.CodigoAsesor;
                Nombre = asesor.Nombre;
                EmpresaId = asesor.EmpresaId;
                Usuario = asesor.Usuario;
                Diario = asesor.Diario;
            }
        }
    }

}
