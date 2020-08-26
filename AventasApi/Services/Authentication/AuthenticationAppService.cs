//using AventasApi.Enviroments;
using DBData.Database;
using AventasApi.Models.Authentication;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
//using Responses;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;
//using IMS.Extensions;

namespace AventasApi.Services.Authentication
{

    public class AuthenticationAppService
    {
        //private readonly AccessPermitionService _accessPermitionService;
        public AuthenticationAppService()
        {
            //_accessPermitionService = new AccessPermitionService();
            encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
            validator = new JwtValidator(serializer, provider);
        }
        const string secret = "BfFbUBWEBRWUxLmzN6CI0sNTkliLbMbzo%LEmZ}@y[.wMv)`lNjOFxUBfFlNjOFxU";

        IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
        IJsonSerializer serializer = new JsonNetSerializer();
        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        IDateTimeProvider provider = new UtcDateTimeProvider();
        IJwtValidator validator;
        IJwtDecoder decoder;
        IJwtEncoder encoder;

        private bool EnLinea(string empresa, string asesor)
        {
            var client = new RestClient(Enviroment.CRMWebServiceURLApi);
            client.Authenticator = new RestSharp.Authenticators.NtlmAuthenticator();
            var request = new RestRequest($"asesor/{empresa}/{asesor}", Method.GET);
            client.Timeout = 6000;
            IRestResponse<List<AsesorApiModel>> respuesta = client.Execute<List<AsesorApiModel>>(request);

            return respuesta.IsSuccessful;
        }

        public AuthenticationResponse Authentication(Credential credential)
        {
            using (var context = new AVentasEntities())
            {
                try
                {
                    string message = string.Empty;
                    if (credential.IsValid(out message) == false)
                        return new AuthenticationResponse { Message = message, Data = null };

                    var userBD = context.Asesores.AsNoTracking().FirstOrDefault(x => x.Usuario.Equals(credential.UserAccount));

                    if (userBD == null)
                        return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };

                    
                    var user = new Usuario { IdUsuario = userBD.Usuario, Pin = null };

                    if (EnLinea(userBD.EmpresaId, userBD.Usuario))
                    {
                        //Validar con usuario de Intermoda
                        var client = new RestClient(string.Format(Enviroment.AuthenticationApi, credential.UserAccount, credential.Password));
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Accept", "application/json");
                        IRestResponse response = client.Execute(request);

                        if (response.IsSuccessful == false)
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }

                        var content = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FailResponse>>(response.Content)[0];
                        if (false && content.Message != "Ok")
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }

                        var entityFound = context.Usuarios.FirstOrDefault(x => x.IdUsuario.Equals(credential.UserAccount));
                        if (entityFound == null)
                        {
                            var newUser = new Usuarios();
                            newUser.IdUsuario = credential.UserAccount;
                            newUser.Password = BCrypt.Net.BCrypt.HashPassword(credential.Password);
                            context.Usuarios.Add(newUser);
                            context.SaveChanges();
                        }
                        else
                        {
                            entityFound.Password = BCrypt.Net.BCrypt.HashPassword(credential.Password);
                            context.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        var entityFound = context.Usuarios.FirstOrDefault(x => x.IdUsuario.Equals(credential.UserAccount));
                        if (entityFound == null)
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }

                        var isSamePassword = BCrypt.Net.BCrypt.Verify(credential.Password, entityFound.Password);
                        if (!isSamePassword)
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }
                    }
                    
                   

                    var token = encoder.Encode(new UserAuthenticated
                    {
                        UserAccount = userBD.Usuario,
                        DueDate = DateTime.Now.AddHours(12)
                    }, secret);

                    var result = new Data {  Token = token, Usuario = user,Empresa=userBD.EmpresaId,Nombre=userBD.Nombre };

                    return new AuthenticationResponse { Type = "1", Message = "Ok", Data = result };
                }
                catch (Exception ex)
                {
                    return new AuthenticationResponse { Message = ex.Message, Data = null };
                }
            }
        }
        public UserAuthenticated Validate(string token)
        {
            return decoder.DecodeToObject<UserAuthenticated>(token);
        }
    }
}