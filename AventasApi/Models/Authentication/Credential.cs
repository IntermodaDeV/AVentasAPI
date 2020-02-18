using System.ComponentModel.DataAnnotations;

namespace AventasApi.Models.Authentication
{
    public class Credential
    {
        public string UserAccount { get; set; }
        public string Password { get; set; }

        public bool IsValid(out string message)
        {
            if (string.IsNullOrWhiteSpace(UserAccount) == true)
            {
                message = "Debe de ingresar un usuario.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password) == true)
            {
                message = "Debe de ingresar una contraseña.";
                return false;
            }

            if (Password.Length <8)
            {
                message = "Contraseña debe ser de al menos 8 carácteres.";
                return false;
            }

            message = "Ok.";
            return true;
        }
    }
}