using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaleOnline.Models
{
    public class HomePageViewModel
    {
        public List<Category> Categories { get; set; }
        public IPagedList<ProductCardViewModel> Products { get; set; }
    }
}