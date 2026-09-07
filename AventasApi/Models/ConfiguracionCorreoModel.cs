namespace AventasApi.Models
{
    public class ConfiguracionCorreoModel
    {
        public int Id { get; set; }
        public string EmpresaId { get; set; }
        public int TipoConfiguracionId { get; set; }
        public string CorreosDestino { get; set; }
        public string CorreosCopia { get; set; }
        public string Asunto { get; set; }
        public string CuerpoPlantilla { get; set; }
        public string Usuario { get; set; }
    }
}
