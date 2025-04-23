using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaleOnline.Models
{
    public class OrderListViewModel
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public decimal FinalPrice { get; set; }
        public string Status { get; set; }
    }
}