using DBData.Database;
using AventasApi.Models.Authentication;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using ExternalApiData.Enviroments;
using ExternalApiData.Models.ApiModels;

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

                    var userBD = context.Usuarios.AsNoTracking().FirstOrDefault(x => x.usuario.Equals(credential.UserAccount));

                    if (userBD == null)
                        return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };

                    if (!userBD.status)
                    {
                        return new AuthenticationResponse { Message = "Usuario se encuentra deshabilitado para el sistema.", Data = null };
                    }

                    if (userBD.SesionActiva == true)
                    {
                        return new AuthenticationResponse { Message = "Usuario cuenta con una sesión activa, acceso denegado.", Data = null };
                    }

                    var user = new Usuario { IdUsuario = userBD.usuario, Pin = null };

                    if (EnLinea(userBD.EmpresaId, userBD.usuario))
                    {
                        //Validar con usuario de Intermoda
                        var client = new RestClient(Enviroment.AuthenticationApi);
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Accept", "application/json");
                        request.AddJsonBody(new {dominio="INTERMODA",usuario=credential.UserAccount,psd=credential.Password});
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

                        var entityFound = context.Usuarios.FirstOrDefault(x => x.usuario.Equals(credential.UserAccount));
                        if (entityFound == null)
                        {
                            var newUser = new Usuarios();
                            newUser.usuario = credential.UserAccount;
                            newUser.password = BCrypt.Net.BCrypt.HashPassword(credential.Password);
                            context.Usuarios.Add(newUser);
                            context.SaveChanges();
                        }
                        else
                        {
                            entityFound.password = BCrypt.Net.BCrypt.HashPassword(credential.Password);
                            context.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        var entityFound = context.Usuarios.FirstOrDefault(x => x.usuario.Equals(credential.UserAccount));
                        if ( entityFound == null)
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }

                        var isSamePassword = BCrypt.Net.BCrypt.Verify(credential.Password, entityFound.password);
                        if (!isSamePassword)
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }
                    }
                    
                   

                    var token = encoder.Encode(new UserAuthenticated
                    {
                        Id=userBD.Id,
                        UserAccount = userBD.usuario,
                        DueDate = DateTime.Now.AddHours(12)
                    }, secret);

                    var result = new Data {  Token = token, Usuario = user,Empresa=userBD.EmpresaId,Nombre=userBD.nombre };

                    return new AuthenticationResponse { Type = "1", Message = "Ok", Data = result };
                }
                catch (Exception ex)
                {
                    return new AuthenticationResponse { Message = ex.Message, Data = null };
                }
            }
        }

        public AuthenticationResponse AuthenticationMovil(Credential credential)
        {
            using (var context = new AVentasEntities())
            {
                try
                {
                    string message = string.Empty;
                    if (credential.IsValid(out message) == false)
                        return new AuthenticationResponse { Message = message, Data = null };

                    var userBD = context.Usuarios.AsNoTracking().FirstOrDefault(x => x.usuario.Equals(credential.UserAccount));

                    if (userBD == null)
                        return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };

                    if (!userBD.status)
                    {
                        return new AuthenticationResponse { Message = "Usuario se encuentra deshabilitado para el sistema.", Data = null };
                    }

                    var user = new Usuario { IdUsuario = userBD.usuario, Pin = null };

                    if (EnLinea(userBD.EmpresaId, userBD.usuario))
                    {
                        //Validar con usuario de Intermoda
                        var client = new RestClient(Enviroment.AuthenticationApi);
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Accept", "application/json");
                        request.AddJsonBody(new {dominio="INTERMODA",usuario=credential.UserAccount,psd=credential.Password});
                        IRestResponse response = client.Execute(request);

                        if (response.IsSuccessful == false)
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }

                        var content = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FailResponse>>(response.Content)[0];
                        if (content.Message != "Ok")
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }

                        var entityFound = context.Usuarios.FirstOrDefault(x => x.usuario.Equals(credential.UserAccount));
                        if (entityFound == null)
                        {
                            var newUser = new Usuarios();
                            newUser.usuario = credential.UserAccount;
                            newUser.password = BCrypt.Net.BCrypt.HashPassword(credential.Password);
                            context.Usuarios.Add(newUser);
                            context.SaveChanges();
                        }
                        else
                        {
                            entityFound.password = BCrypt.Net.BCrypt.HashPassword(credential.Password);
                            context.Entry(entityFound).State = System.Data.Entity.EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        var entityFound = context.Usuarios.FirstOrDefault(x => x.usuario.Equals(credential.UserAccount));
                        if (entityFound == null)
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }

                        var isSamePassword = BCrypt.Net.BCrypt.Verify(credential.Password, entityFound.password);
                        if (!isSamePassword)
                        {
                            return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                        }
                    }



                    var token = encoder.Encode(new UserAuthenticated
                    {
                        Id = userBD.Id,
                        UserAccount = userBD.usuario,
                        DueDate = DateTime.Now.AddHours(12)
                    }, secret);

                    var result = new Data { Token = token, Usuario = user, Empresa = userBD.EmpresaId, Nombre = userBD.nombre };

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