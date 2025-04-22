using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaleOnline.Models
{
    public class HomePageViewModel
    {
        public List<Category> Categories { get; set; }
        public List<ProductViewModel> Products { get; set; }
    }
}