using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaleOnline.Models
{
    public class ProductCardViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public long QuantitySold { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
}