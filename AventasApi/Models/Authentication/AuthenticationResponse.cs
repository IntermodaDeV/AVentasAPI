using System.Collections.Generic;

namespace AventasApi.Models.Authentication
{
    public class AuthenticationResponse
    {
        public string Token { get; set; }
        public Usuario Usuario  { get; set; }
        public List<Menu> Accesos  { get; set; }
    }

    public class FailResponse
    {
        public string Message { get; set; }
    }
}