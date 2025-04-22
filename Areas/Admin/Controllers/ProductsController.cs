using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        SaleOnlineEntities db = new SaleOnlineEntities();
        // GET: Admin/Products
        public ActionResult Index(string search, long? categoryId, string status, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var products = db.Products.Include("Category").AsQueryable();

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search));

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(status))
                products = products.Where(p => p.Available == status);

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentStatus = status;

            ViewBag.Categories = db.Categories.OrderBy(c => c.Name).ToList(); // ✅ cần thiết

            var paged = products.OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = db.Categories.ToList();
                Response.StatusCode = 400;
                return PartialView("_AddProductPartial", model);
            }

            string imagePath = null;
            if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
            {
                string fileName = Path.GetFileName(model.ImageFile.FileName);
                string path = Path.Combine(Server.MapPath("~/Content/product-images"), fileName);
                model.ImageFile.SaveAs(path);
                imagePath = "~/Content/product-images/" + fileName;
            }

            var product = new Product
            {
                Name = model.Name,
                ProductionDate = model.ProductionDate,
                Description = model.Description,
                Image = imagePath ?? "",
                QuantitySold = model.QuantitySold,
                CategoryId = model.CategoryId,
                Available = model.Available
            };

            db.Products.Add(product);
            db.SaveChanges();

            var variation = new Variation
            {
                ProductId = product.Id,
                Name = model.VariationName,
                Available = model.VariationAvailable
            };

            db.Variations.Add(variation);
            db.SaveChanges();

            // Duyệt từng option
            for (int i = 0; i < model.Options.Count; i++)
            {
                var opt = model.Options[i];
                string optionImagePath = "";

                var file = Request.Files[$"Options[{i}].ImageFile"];
                if (file != null && file.ContentLength > 0)
                {
                    string filename = Path.GetFileName(file.FileName);
                    string savePath = Path.Combine(Server.MapPath("~/Content/option-images"), filename);
                    file.SaveAs(savePath);
                    optionImagePath = "~/Content/option-images/" + filename;
                }

                db.VariationOptions.Add(new VariationOption
                {
                    VariationId = variation.Id,
                    Value = opt.Value,
                    Image = optionImagePath,
                    Stock = opt.Stock,
                    OriginalPrice = opt.OriginalPrice,
                    SellingPrice = opt.SellingPrice,
                    Discount = opt.Discount,
                    FinalPrice = opt.FinalPrice,
                    Available = opt.Available
                });
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        public ActionResult GetProductTable(string search, long? categoryId, string status, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var products = db.Products.Include("Category").AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                products = products.Where(p => p.Available == status);
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentStatus = status;

            var paged = products.OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize);
            return PartialView("_ProductTablePartial", paged);
        }
        public ActionResult DetailPartial(long id)
        {
            var product = db.Products
                .Include("Category")
                .Include("Variations.VariationOptions")
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return HttpNotFound();

            return PartialView("_ProductDetailPartial", product);
        }
    }
}