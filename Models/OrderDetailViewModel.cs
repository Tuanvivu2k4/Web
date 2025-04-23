using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaleOnline.Models
{
    public class OrderDetailViewModel
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal FinalPrice { get; set; }
        public string Status { get; set; }
        public string VoucherCode { get; set; }
        public decimal Discount { get; set; }

        public List<OrderItemViewModel> Items { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public string VariationOptionValue { get; set; }
        public int Quantity { get; set; }
        public decimal PriceItem { get; set; }
    }
}