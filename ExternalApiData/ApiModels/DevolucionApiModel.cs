using System.Collections.Generic;

namespace ExternalApiData.ApiModels
{
    public class DevolucionApiModel
    {
        public DevolucionApiModel()
        {
            DevolucionDetalleJson = new List<DevolucionDetalleJson>();
        }
        public string COMPANY { get; set; }
        public string CUSTOMER_ACCOUNT { get; set; }
        public string SALES_MANAGER { get; set; }
        public string USER { get; set; }
        public string OBSERVATIONS { get; set; }
        public string REASON_CODE { get; set; }
        public string REFERENCE { get; set; }
        public string SALES_NAME { get; set; }
        public string LINE { get; set; }
        public List<DevolucionDetalleJson> DevolucionDetalleJson { get; set; }
    }
}
