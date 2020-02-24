using AventasApi.Models.Authentication;
using AventasApi.Services.Authentication;
//using IMS.Tokens.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace AventasApi.Filters
{
    public class Auth : AuthorizationFilterAttribute
    {
        AuthenticationAppService TokenService = new AuthenticationAppService();
        private bool ValidateToken(string token, out string message)
        {
            try
            {
                var user = TokenService.Validate(token);

                if (user.DueDate <= DateTime.Now)
                {
                    message = "The token is expired, restart the session.";
                    return false;
                }

                message = "Ok";
                return true;
            }
            catch (Exception ex)
            {
                message = "Token not valid.";
                return false;
            }
        }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var header = actionContext.Request.Headers.Authorization;
            var message = string.Empty;

            if (header == null)
                HandleUnauthorized(actionContext, "Acceso no autorizado.");
            else if (ValidateToken(header.Parameter, out message) == false)
                HandleUnauthorized(actionContext, message);
        }

        void HandleUnauthorized(HttpActionContext actionContext, string message)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);

            actionContext.Response.Content = new ObjectContent<object>(new { Message = message }, new JsonMediaTypeFormatter(), "application/json"); ;
        }
    }
}