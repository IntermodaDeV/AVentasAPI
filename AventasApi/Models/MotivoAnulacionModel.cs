namespace AventasApi.Models
{
    public class MotivoAnulacionModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public string Usuario { get; set; }
    }
}
