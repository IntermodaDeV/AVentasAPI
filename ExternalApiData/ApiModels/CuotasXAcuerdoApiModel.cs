using System;

namespace ExternalApiData.ApiModels
{
    public class CuotasXAcuerdoApiModel
    {
        public int IMPAYMENTNUMBER { get; set; }
        public decimal AMOUNT { get; set; }
        public DateTime DUEDATE { get; set; }
        public decimal REMAINAMOUNT { get; set; }
    }
}
