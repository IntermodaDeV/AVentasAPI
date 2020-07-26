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



        public AuthenticationResponse Authentication(Credential credential)
        {
            using (var context = new AVentasEntities())
            {
                try
                {
                    string message = string.Empty;
                    if (credential.IsValid(out message) == false)
                        return new AuthenticationResponse { Message = message, Data = null };

                    //var userBD = context.Usuarios.AsNoTracking().FirstOrDefault(x => x.IdUsuario.Equals(credential.UserAccount));
                    var userBD = context.Asesores.AsNoTracking().FirstOrDefault(x => x.Usuario.Equals(credential.UserAccount));

                    if (userBD == null)
                        return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };

                    //var forms = context.Pantallas.ToList();

                    //var accessUser = context.PantallasUsuarios    
                    //                        .Where(access => access.IdUsuario.Equals(userBD.IdUsuario) && access.Activa == true).ToList();

                    //var menu = _accessPermitionService.GetAccessPermission(accessUser, forms, "tree");

                    //var user = new Usuario { IdUsuario = userBD.IdUsuario, Pin = userBD.Pin };
                    var user = new Usuario { IdUsuario = userBD.Usuario, Pin = null };
                    //{
                    //    var token2 = encoder.Encode(new UserAuthenticated
                    //    {
                    //        UserAccount = userBD.Usuario,
                    //        DueDate = DateTime.Now.AddHours(12)
                    //    }, secret);

                    //    var result2 = new AuthenticationResponse { Token = token2, Usuario = user };
                    //    //var result = new AuthenticationResponse { Token = token, Usuario = user };                    
                    //    //var result = new AuthenticationResponse { Token = token, Usuario = user, Accesos = menu };

                    //    return new AuthenticationResponse { Type = TypeResponse.Ok, Message = "Ok", Data = result2 };
                    //}
                    //Validar con usuario de Intermoda
                    var client = new RestClient(string.Format(Enviroment.AuthenticationApi, credential.UserAccount, credential.Password));
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Accept", "application/json");
                    //request.AddJsonBody(new { userAccount = credential.UserAccount, password = credential.Password });
                    //Se regresa aca para evitar la validacion con ax, ya que el server se encuentra bajo mantenimiento.
                    //return new AuthenticationResponse { Type = TypeResponse.Ok, Message = "Ok", Data = new AuthenticationResponse{ Token = "token?", Usuario = user } };

                    IRestResponse response = client.Execute(request);
                    if (response.IsSuccessful == false)
                    {
                        return new AuthenticationResponse { Message = "Usuario o contraseña incorrectos.", Data = null };
                    }
                    var content = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FailResponse>>(response.Content)[0];
                    if (false && content.Message != "Ok")
                    {
                        throw new Exception("Usuario o contraseña incorectos");

                    }
                    //if (response.Data.Type != TypeResponse.Ok)
                    //return new AuthenticationResponse { Type = response.Data.Type, Message = response.Data.Message, Data = null };

                    var token = encoder.Encode(new UserAuthenticated
                    {
                        UserAccount = userBD.Usuario,
                        DueDate = DateTime.Now.AddHours(12)
                    }, secret);

                    var result = new Data {  Token = token, Usuario = user,Empresa=userBD.EmpresaId,Nombre=userBD.Nombre };
                    //var result = new AuthenticationResponse { Token = token, Usuario = user };                    
                    //var result = new AuthenticationResponse { Token = token, Usuario = user, Accesos = menu };

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