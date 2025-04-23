using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaleOnline.Models
{
    public class CartItemViewModel
    {
        public long CartItemId { get; set; }
        public string ProductName { get; set; }
        public string VariationOptionName { get; set; }
        public string ImageUrl { get; set; }
        public decimal FinalPrice { get; set; }
        public int Quantity { get; set; }
        public long Stock { get; set; } // ✅ Thêm dòng này
        public decimal TotalPrice => FinalPrice * Quantity;
    }
}