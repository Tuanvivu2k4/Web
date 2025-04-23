using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;
using OfficeOpenXml;
using System.Text;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        // Hàm tách CSV dòng, hỗ trợ dấu phẩy trong chuỗi bằng dấu ngoặc kép
        private string[] SplitCsvLine(string line)
        {
            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
            return System.Text.RegularExpressions.Regex.Split(line, pattern)
                .Select(s => s.Trim().Trim('"')).ToArray();
        }
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

        public ActionResult EditPartial(long id)
        {
            var product = db.Products
                .Include("Category")
                .Include("Variations.VariationOptions")
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return HttpNotFound();

            var variation = product.Variations.FirstOrDefault();

            var model = new EditProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                ProductionDate = product.ProductionDate,
                Description = product.Description,
                Image = product.Image,
                CategoryId = product.CategoryId,
                QuantitySold = product.QuantitySold,
                Available = product.Available,
                VariationName = variation?.Name,
                VariationAvailable = variation?.Available,
                Options = variation?.VariationOptions.Select(opt => new VariationOptionViewModel1
                {
                    Id = opt.Id,
                    Value = opt.Value,
                    Image = opt.Image,
                    Stock = opt.Stock,
                    OriginalPrice = opt.OriginalPrice,
                    SellingPrice = opt.SellingPrice,
                    Discount = opt.Discount,
                    FinalPrice = opt.FinalPrice,
                    Available = opt.Available
                }).ToList()
            };

            ViewBag.Categories = db.Categories.ToList();
            return PartialView("_EditProductPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = db.Categories.ToList();
                Response.StatusCode = 400;
                return PartialView("_EditProductPartial", model);
            }

            var product = db.Products.Find(model.Id);
            if (product == null) return HttpNotFound();

            // Update product fields
            product.Name = model.Name;
            product.ProductionDate = model.ProductionDate;
            product.Description = model.Description;
            product.CategoryId = model.CategoryId;
            product.Available = model.Available;

            // Image update
            if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
            {
                string fileName = Path.GetFileName(model.ImageFile.FileName);
                string path = Path.Combine(Server.MapPath("~/Content/product-images"), fileName);
                model.ImageFile.SaveAs(path);
                product.Image = "~/Content/product-images/" + fileName;
            }

            // Handle variation
            var variation = db.Variations.FirstOrDefault(v => v.ProductId == product.Id);
            if (variation == null)
            {
                variation = new Variation
                {
                    ProductId = product.Id
                };
                db.Variations.Add(variation);
                db.SaveChanges();
            }

            variation.Name = model.VariationName;
            variation.Available = model.VariationAvailable;

            // Existing option IDs
            var existingOptionIds = model.Options.Where(o => o.Id != 0).Select(o => o.Id).ToList();

            // Delete removed options
            var optionsToDelete = db.VariationOptions
                .Where(o => o.VariationId == variation.Id && !existingOptionIds.Contains(o.Id))
                .ToList();

            db.VariationOptions.RemoveRange(optionsToDelete);

            // Update or insert options
            for (int i = 0; i < model.Options.Count; i++)
            {
                var optModel = model.Options[i];
                VariationOption option;

                if (optModel.Id != 0)
                {
                    option = db.VariationOptions.Find(optModel.Id);
                    if (option == null) continue;
                }
                else
                {
                    option = new VariationOption { VariationId = variation.Id };
                    db.VariationOptions.Add(option);
                }

                option.Value = optModel.Value;
                option.Stock = optModel.Stock;
                option.OriginalPrice = optModel.OriginalPrice;
                option.SellingPrice = optModel.SellingPrice;
                option.Discount = optModel.Discount;
                option.FinalPrice = optModel.FinalPrice;
                option.Available = optModel.Available;

                // Save image
                var file = Request.Files[$"Options[{i}].ImageFile"];
                if (file != null && file.ContentLength > 0)
                {
                    string filename = Path.GetFileName(file.FileName);
                    string savePath = Path.Combine(Server.MapPath("~/Content/option-images"), filename);
                    file.SaveAs(savePath);
                    option.Image = "~/Content/option-images/" + filename;
                }
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteProduct(long id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            db.Products.Remove(product); // Xoá cả Variation & Option qua cascade
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteSelectedProducts(List<long> ids)
        {
            var products = db.Products.Where(p => ids.Contains(p.Id)).ToList();
            db.Products.RemoveRange(products);
            db.SaveChanges();
            return Json(new { success = true });
        }

        public ActionResult Export(string format)
        {
            var products = db.Products
                .Include("Category")
                .Include("Variations.VariationOptions")
                .OrderBy(p => p.Id)
                .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            switch (format.ToLower())
            {
                case "csv":
                    var csv = new StringBuilder();
                    csv.AppendLine("ProductId,ProductName,Category,Variation,OptionValue,Stock,OriginalPrice,SellingPrice,Discount,FinalPrice,Status");

                    foreach (var p in products)
                    {
                        var variation = p.Variations.FirstOrDefault();
                        if (variation != null)
                        {
                            foreach (var opt in variation.VariationOptions)
                            {
                                csv.AppendLine($"{p.Id},\"{p.Name}\",\"{p.Category?.Name}\",\"{variation.Name}\",\"{opt.Value}\",{opt.Stock},{opt.OriginalPrice},{opt.SellingPrice},{opt.Discount},{opt.FinalPrice},\"{opt.Available}\"");
                            }
                        }
                    }

                    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "products.csv");

                case "txt":
                    var lines = new List<string>
            {
                "ProductId\tProductName\tCategory\tVariation\tOptionValue\tStock\tOriginalPrice\tSellingPrice\tDiscount\tFinalPrice\tStatus"
            };

                    foreach (var p in products)
                    {
                        var variation = p.Variations.FirstOrDefault();
                        if (variation != null)
                        {
                            foreach (var opt in variation.VariationOptions)
                            {
                                lines.Add($"{p.Id}\t{p.Name}\t{p.Category?.Name}\t{variation.Name}\t{opt.Value}\t{opt.Stock}\t{opt.OriginalPrice}\t{opt.SellingPrice}\t{opt.Discount}\t{opt.FinalPrice}\t{opt.Available}");
                            }
                        }
                    }

                    return File(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)), "text/plain", "products.txt");

                case "xlsx":
                default:
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("Products");
                        ws.Cells[1, 1].Value = "ProductId";
                        ws.Cells[1, 2].Value = "ProductName";
                        ws.Cells[1, 3].Value = "Category";
                        ws.Cells[1, 4].Value = "Variation";
                        ws.Cells[1, 5].Value = "OptionValue";
                        ws.Cells[1, 6].Value = "Stock";
                        ws.Cells[1, 7].Value = "OriginalPrice";
                        ws.Cells[1, 8].Value = "SellingPrice";
                        ws.Cells[1, 9].Value = "Discount";
                        ws.Cells[1, 10].Value = "FinalPrice";
                        ws.Cells[1, 11].Value = "Status";

                        int row = 2;
                        foreach (var p in products)
                        {
                            var variation = p.Variations.FirstOrDefault();
                            if (variation != null)
                            {
                                foreach (var opt in variation.VariationOptions)
                                {
                                    ws.Cells[row, 1].Value = p.Id;
                                    ws.Cells[row, 2].Value = p.Name;
                                    ws.Cells[row, 3].Value = p.Category?.Name;
                                    ws.Cells[row, 4].Value = variation.Name;
                                    ws.Cells[row, 5].Value = opt.Value;
                                    ws.Cells[row, 6].Value = opt.Stock;
                                    ws.Cells[row, 7].Value = opt.OriginalPrice;
                                    ws.Cells[row, 8].Value = opt.SellingPrice;
                                    ws.Cells[row, 9].Value = opt.Discount;
                                    ws.Cells[row, 10].Value = opt.FinalPrice;
                                    ws.Cells[row, 11].Value = opt.Available;
                                    row++;
                                }
                            }
                        }

                        var file = package.GetAsByteArray();
                        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx");
                    }
            }
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase importFile)
        {
            if (importFile == null || importFile.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file để upload.";
                return RedirectToAction("Index");
            }

            string ext = Path.GetExtension(importFile.FileName).ToLower();

            try
            {
                var lines = new List<string[]>();
                if (ext == ".xlsx")
                {
                    using (var package = new ExcelPackage(importFile.InputStream))
                    {
                        var ws = package.Workbook.Worksheets.FirstOrDefault();
                        if (ws == null) throw new Exception("File Excel không hợp lệ.");

                        for (int row = 2; row <= ws.Dimension.End.Row; row++)
                        {
                            lines.Add(new[]
                            {
                        ws.Cells[row, 2].Text, // ProductName
                        ws.Cells[row, 3].Text, // Category
                        ws.Cells[row, 4].Text, // Variation
                        ws.Cells[row, 5].Text, // OptionValue
                        ws.Cells[row, 6].Text, // Stock
                        ws.Cells[row, 7].Text, // OriginalPrice
                        ws.Cells[row, 8].Text, // SellingPrice
                        ws.Cells[row, 9].Text, // Discount
                        ws.Cells[row, 10].Text, // FinalPrice
                        ws.Cells[row, 11].Text // Status
                    });
                        }
                    }
                }
                else
                {
                    using (var reader = new StreamReader(importFile.InputStream))
                    {
                        bool isHeader = true;
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (isHeader) { isHeader = false; continue; }

                            var values = ext == ".csv"
                                ? SplitCsvLine(line)
                                : line.Split('\t');
                            lines.Add(values);
                        }
                    }
                }

                foreach (var v in lines)
                {
                    if (v.Length < 10) continue;

                    string productName = v[0];
                    string categoryName = v[1];
                    string variationName = v[2];
                    string optionValue = v[3];

                    if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(optionValue)) continue;

                    var category = db.Categories.FirstOrDefault(c => c.Name == categoryName)
                                   ?? db.Categories.Add(new Category { Name = categoryName, Image = "", Available = "Active" });

                    db.SaveChanges();

                    var product = db.Products.FirstOrDefault(p => p.Name == productName)
                                  ?? db.Products.Add(new Product
                                  {
                                      Name = productName,
                                      CategoryId = category.Id,
                                      ProductionDate = DateTime.Now,
                                      Description = "",
                                      Image = "",
                                      QuantitySold = 0,
                                      Available = "Active"
                                  });

                    db.SaveChanges();

                    var variation = db.Variations.FirstOrDefault(vr => vr.ProductId == product.Id)
                                    ?? db.Variations.Add(new Variation
                                    {
                                        ProductId = product.Id,
                                        Name = variationName,
                                        Available = "Active"
                                    });

                    db.SaveChanges();

                    db.VariationOptions.Add(new VariationOption
                    {
                        VariationId = variation.Id,
                        Value = optionValue,
                        Stock = long.TryParse(v[4], out var stock) ? stock : 0,
                        OriginalPrice = decimal.TryParse(v[5], out var o) ? o : 0,
                        SellingPrice = decimal.TryParse(v[6], out var s) ? s : 0,
                        Discount = decimal.TryParse(v[7], out var d) ? d : 0,
                        FinalPrice = decimal.TryParse(v[8], out var f) ? f : 0,
                        Available = v[9] ?? "Active",
                        Image = ""
                    });

                    db.SaveChanges();
                }

                TempData["SuccessMessage"] = "Import sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi import: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}