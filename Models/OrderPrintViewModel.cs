using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaleOnline.Models
{
    public class OrderPrintViewModel
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public List<OrderPrintItem> Items { get; set; }
        public decimal Total { get; set; }
        public decimal AmountToPay { get; set; }
    }

    public class OrderPrintItem
    {
        public string ProductName { get; set; }
        public string Variation { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}