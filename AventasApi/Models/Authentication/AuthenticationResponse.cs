using System.Collections.Generic;

namespace AventasApi.Models.Authentication
{
    public class AuthenticationResponse
    {
        public Data Data { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }

    }

    public class FailResponse
    {
        public string Message { get; set; }
    }
    public class Data{
        public string Token { get; set; }
        public Usuario Usuario { get; set; }
        public string Empresa { get; set; }
        public List<Menu> Accesos { get; set; }
    }
}