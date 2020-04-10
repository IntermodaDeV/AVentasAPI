using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalApiData.Models.ApiModels
{
    public class ImageneXProductoXColorApiModel
    {
        public string ITEM_CODE { get; set; }
        public string ITEM_COLOR { get; set; }
        public string IMAGE_PATH { get; set; }
        public string IMAGE_MAIN { get; set; }
    }
}