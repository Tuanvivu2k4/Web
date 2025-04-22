using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using PagedList;
using OfficeOpenXml;
using System.Text;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class CategoriesController : BaseAdminController
    {
        SaleOnlineEntities db = new SaleOnlineEntities();
        // GET: Admin/Category
        public ActionResult Index(int? page, string search)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = db.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                string lowered = search.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lowered));
            }

            var list = query.OrderBy(c => c.Id).ToPagedList(pageNumber, pageSize);
            return View(list);
        }

        // Tải lại bảng sau khi thêm
        public PartialViewResult GetCategoryTable()
        {
            var list = db.Categories.ToList();
            return PartialView("_CategoryTablePartial", list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = null;
                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/category-images"), fileName);
                    model.ImageFile.SaveAs(path);
                    imagePath = "~/Content/category-images/" + fileName;
                }
                var newCategory = new Category
                {
                    Name = model.Name,
                    Image = imagePath,
                    Available = model.Available
                };

                db.Categories.Add(newCategory);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Category added successfully!";
                return RedirectToAction("Index", "Categories", new { area = "Admin" });
            }

            Response.StatusCode = 400;
            return PartialView("_AddCategoryPartial", model);
        }

        [HttpPost]
        public ActionResult Delete(long id)
        {
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            // Nếu có ràng buộc, kiểm tra tại đây (ví dụ: nếu có sản phẩm thuộc category thì không cho xoá)
            if (category.Products.Any()) return new HttpStatusCodeResult(400, "Category has products.");

            db.Categories.Remove(category);
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteSelected(List<long> ids)
        {
            foreach (var id in ids)
            {
                var category = db.Categories.Find(id);
                if (category != null && !category.Products.Any())
                {
                    db.Categories.Remove(category);
                }
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        public ActionResult EditPartial(long id)
        {
            var category = db.Categories.Find(id);
            if (category == null)
                return HttpNotFound();

            var model = new EditCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Available = category.Available,
                Image = category.Image // cần có trong ViewModel
            };

            return PartialView("_EditCategoryPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Categories.Find(model.Id);
                if (existing == null)
                    return HttpNotFound();

                bool hasChanges = false;

                if (existing.Name != model.Name)
                {
                    existing.Name = model.Name;
                    hasChanges = true;
                }

                if (existing.Available != model.Available)
                {
                    existing.Available = model.Available;
                    hasChanges = true;
                }

                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/category-images"), fileName);
                    model.ImageFile.SaveAs(path);
                    existing.Image = "~/Content/category-images/" + fileName;
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    Response.StatusCode = 400;
                    ModelState.AddModelError("", "Không có thay đổi nào được thực hiện.");
                    return PartialView("_EditCategoryPartial", model);
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction("Index", "Categories", new { area = "Admin" });
            }

            Response.StatusCode = 400;
            return PartialView("_EditCategoryPartial", model);
        }


        public ActionResult Export(string format)
        {
            var categories = db.Categories.OrderBy(c => c.Id).ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            switch (format.ToLower())
            {
                case "csv":
                    var csv = new StringBuilder();
                    csv.AppendLine("Id,Name,Image,Available");
                    foreach (var cat in categories)
                    {
                        csv.AppendLine($"{cat.Id},\"{cat.Name}\",\"{cat.Image}\",\"{cat.Available}\"");
                    }

                    byte[] bufferCsv = Encoding.UTF8.GetBytes(csv.ToString());
                    return File(bufferCsv, "text/csv", "categories.csv");

                case "txt":
                    var lines = new List<string> { "Id\tName\tImage\tAvailable" };
                    lines.AddRange(categories.Select(c => $"{c.Id}\t{c.Name}\t{c.Image}\t{c.Available}"));
                    byte[] bufferTxt = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
                    return File(bufferTxt, "text/plain", "categories.txt");

                case "xlsx":
                default:
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("Categories");
                        ws.Cells[1, 1].Value = "Id";
                        ws.Cells[1, 2].Value = "Name";
                        ws.Cells[1, 3].Value = "Image";
                        ws.Cells[1, 4].Value = "Available";

                        for (int i = 0; i < categories.Count; i++)
                        {
                            var c = categories[i];
                            ws.Cells[i + 2, 1].Value = c.Id;
                            ws.Cells[i + 2, 2].Value = c.Name;
                            ws.Cells[i + 2, 3].Value = c.Image;
                            ws.Cells[i + 2, 4].Value = c.Available;
                        }

                        var file = package.GetAsByteArray();
                        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "categories.xlsx");
                    }
            }
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase importFile)
        {
            if (importFile == null || importFile.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction("Index");
            }

            string fileExt = Path.GetExtension(importFile.FileName).ToLower();

            try
            {
                if (fileExt == ".xlsx")
                {
                    using (var package = new ExcelPackage(importFile.InputStream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null) throw new Exception("Invalid Excel file.");

                        int rowCount = worksheet.Dimension.End.Row;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            string name = worksheet.Cells[row, 2].Text?.Trim();
                            string image = worksheet.Cells[row, 3].Text?.Trim();
                            string available = worksheet.Cells[row, 4].Text?.Trim();

                            if (string.IsNullOrWhiteSpace(name)) continue;

                            db.Categories.Add(new Category
                            {
                                Name = name,
                                Image = image ?? "",
                                Available = available ?? "Yes"
                            });
                        }
                    }
                }
                else if (fileExt == ".csv" || fileExt == ".txt")
                {
                    using (var reader = new StreamReader(importFile.InputStream))
                    {
                        bool isHeader = true;
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (isHeader) { isHeader = false; continue; } // Skip header

                            var values = fileExt == ".csv"
                                ? SplitCsvLine(line)
                                : line.Split('\t');

                            if (values.Length < 4) continue;

                            string name = values[1].Trim();
                            string image = values[2].Trim();
                            string available = values[3].Trim();

                            if (string.IsNullOrWhiteSpace(name)) continue;

                            db.Categories.Add(new Category
                            {
                                Name = name,
                                Image = image ?? "",
                                Available = available ?? "Yes"
                            });
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Unsupported file format.";
                    return RedirectToAction("Index");
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Import successful!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Import failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Tách dòng CSV đúng cách
        private string[] SplitCsvLine(string line)
        {
            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
            return System.Text.RegularExpressions.Regex.Split(line, pattern);
        }
    }
}