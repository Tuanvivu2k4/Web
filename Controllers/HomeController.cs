using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SaleOnline.Controllers
{
    public class HomeController : Controller
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();

        public ActionResult Index()
        {
            var categories = db.Categories
                .Where(c => c.Available.ToLower() == "active")
                .ToList();

            var products = db.Products
                .Where(p => p.Available.ToLower() == "active")
                .OrderByDescending(p => p.QuantitySold)
                .Take(12)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Image,
                    p.QuantitySold,
                    Options = p.Variations
                                .Where(v => v.Available.ToLower() == "active")
                                .SelectMany(v => v.VariationOptions
                                    .Where(o => o.Available.ToLower() == "active"))
                })
                .ToList()
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    QuantitySold = p.QuantitySold,
                    MinPrice = p.Options.Min(o => o.FinalPrice),
                    MaxPrice = p.Options.Max(o => o.FinalPrice)
                })
                .ToList();

            var viewModel = new HomePageViewModel
            {
                Categories = categories,
                Products = products
            };

            return View(viewModel);
        }
    }
}