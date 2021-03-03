using System;

namespace ShopifyWebhook.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public string OrderId { get; set; }
    }
}