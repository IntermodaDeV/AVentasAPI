namespace AventasApi.Models
{
    public class ConfiguracionCorreoUsuarioModel
    {
        public int ConfiguracionCorreoId { get; set; }
        public int? UsuarioId { get; set; }
        public string CorreoManual { get; set; }
        public string Tipo { get; set; }
        public string Usuario { get; set; }
    }
}
