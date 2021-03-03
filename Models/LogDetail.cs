using System;

namespace ShopifyWebhook.Models
{
    public class LogDetail
    {
        public int Id { get; set; }
        public string Error { get; set; }
        public Guid LogId { get; set; }
    }
}
