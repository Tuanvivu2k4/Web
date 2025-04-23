using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

namespace SaleOnline.Controllers
{
    public class HomeController : Controller
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();

        public ActionResult Index(int? page)
        {
            int pageSize = 30;
            int pageNumber = page ?? 1;

            var categories = db.Categories
                .Where(c => c.Available.ToLower() == "active")
                .ToList();

            var allProducts = db.Products
                .Where(p => p.Available.ToLower() == "active")
                .OrderByDescending(p => p.QuantitySold)
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
                .Select(p => new ProductCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    QuantitySold = p.QuantitySold,
                    MinPrice = p.Options.Min(o => o.FinalPrice),
                    MaxPrice = p.Options.Max(o => o.FinalPrice)
                })
                .ToPagedList(pageNumber, pageSize);

            var viewModel = new HomePageViewModel
            {
                Categories = categories,
                Products = allProducts
            };

            return View(viewModel);
        }

        public ActionResult BrowseCategories()
        {
            var categories = db.Categories
                .Where(c => c.Available == "Active")
                .ToList();

            return PartialView("_CategoryGrid", categories);
        }

        public ActionResult TopSellingProducts()
        {
            var products = db.Products
                .Where(p => p.Available == "Active")
                .OrderByDescending(p => p.QuantitySold)
                .Take(10)
                .Select(p => new ProductCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    QuantitySold = p.QuantitySold,
                    MinPrice = db.Variations
                                .Where(v => v.ProductId == p.Id && v.Available == "Active")
                                .SelectMany(v => v.VariationOptions)
                                .Where(vo => vo.Available == "Active")
                                .Min(vo => (decimal?)vo.FinalPrice) ?? 0,
                    MaxPrice = db.Variations
                                .Where(v => v.ProductId == p.Id && v.Available == "Active")
                                .SelectMany(v => v.VariationOptions)
                                .Where(vo => vo.Available == "Active")
                                .Max(vo => (decimal?)vo.FinalPrice) ?? 0
                })
                .ToList();

            ViewBag.IdPrefix = "top-selling";
            return PartialView("_ProductSlider", products);
        }

        public ActionResult NewArrivals()
        {
            var products = db.Products
                .Where(p => p.Available == "Active")
                .OrderByDescending(p => p.ProductionDate)
                .Take(10)
                .Select(p => new ProductCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    QuantitySold = p.QuantitySold,
                    MinPrice = db.Variations
                                .Where(v => v.ProductId == p.Id && v.Available == "Active")
                                .SelectMany(v => v.VariationOptions)
                                .Where(vo => vo.Available == "Active")
                                .Min(vo => (decimal?)vo.FinalPrice) ?? 0,
                    MaxPrice = db.Variations
                                .Where(v => v.ProductId == p.Id && v.Available == "Active")
                                .SelectMany(v => v.VariationOptions)
                                .Where(vo => vo.Available == "Active")
                                .Max(vo => (decimal?)vo.FinalPrice) ?? 0
                })
                .ToList();

            ViewBag.IdPrefix = "new-arrivals";
            return PartialView("_ProductSlider", products);
        }

        public ActionResult LoadProducts(int page = 1, int pageSize = 16)
        {
            var products = db.Products
                .Where(p => p.Available == "Active")
                .OrderByDescending(p => p.ProductionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    QuantitySold = p.QuantitySold,
                    MinPrice = db.Variations
                                .Where(v => v.ProductId == p.Id && v.Available == "Active")
                                .SelectMany(v => v.VariationOptions)
                                .Where(vo => vo.Available == "Active")
                                .Min(vo => (decimal?)vo.FinalPrice) ?? 0,
                    MaxPrice = db.Variations
                                .Where(v => v.ProductId == p.Id && v.Available == "Active")
                                .SelectMany(v => v.VariationOptions)
                                .Where(vo => vo.Available == "Active")
                                .Max(vo => (decimal?)vo.FinalPrice) ?? 0
                })
                .ToList();

            return PartialView("_ProductGridPartial", products);
        }
    }
}