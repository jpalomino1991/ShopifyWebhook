﻿using System;

namespace ShopifyWebhook.Models
{
    public class Logs
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string Status { get; set; }
        public string Detail { get; set; }
    }
}
