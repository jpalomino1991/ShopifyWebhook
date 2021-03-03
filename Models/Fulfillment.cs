using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopifyWebhook.Models
{
    public class Fulfillment
    {
        public string id { get; set; }
        public string order_id { get; set; }
        public string location_id { get; set; }
        public string tracking_number { get; set; }
    }
}