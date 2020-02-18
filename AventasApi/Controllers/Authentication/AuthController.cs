using AventasApi.Models.Authentication;
using AventasApi.Services.Authentication;
using Responses;
using System.Collections.Generic;
using System.Web.Http;

namespace AventasApi.Controllers.Authentication
{
    [RoutePrefix("api")]
    public class AuthController : ApiController
    {
        private readonly AuthenticationAppService _authenticationAppService;
        public AuthController()
        {
            _authenticationAppService = new AuthenticationAppService();
        }

        [HttpPost, Route("authentication")]
        public IHttpActionResult Authentication([FromBody] Credential credential)
        {
            var answer = _authenticationAppService.Authentication(credential);

            if (answer.Type == TypeResponse.ErrorValidation)
            {
                return BadRequest(answer.Message);
            }

            return Ok(answer);
        }
    }
}
