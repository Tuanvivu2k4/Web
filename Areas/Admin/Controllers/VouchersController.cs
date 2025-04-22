using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using OfficeOpenXml;
using System.Text;
using System.IO;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class VouchersController : BaseAdminController
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();
        // GET: Admin/Vouchers
        public ActionResult Index(string searchCode, string status, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var vouchers = db.Vouchers.AsQueryable();

            if (!string.IsNullOrEmpty(searchCode))
            {
                string lowered = searchCode.ToLower();
                vouchers = vouchers.Where(e => e.Code.ToLower().Contains(lowered));
            }

            if (!string.IsNullOrEmpty(status))
            {
                vouchers = vouchers.Where(v => v.Available == status);
            }

            ViewBag.CurrentSearchCode = searchCode;
            ViewBag.CurrentStatus = status;

            var paged = vouchers.OrderBy(e => e.Id).ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateVoucherViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool codeExists = db.Vouchers.Any(v => v.Code == model.Code);
                if (codeExists)
                {
                    ModelState.AddModelError("Code", "Mã voucher đã tồn tại.");
                    Response.StatusCode = 400;
                    return PartialView("_AddVoucherPartial", model);
                }

                var voucher = new Voucher
                {
                    Code = model.Code,
                    Discount = model.Discount,
                    Quantity = model.Quantity,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Available = model.Available
                };

                db.Vouchers.Add(voucher);
                db.SaveChanges();

                return RedirectToAction("Index", "Vouchers", new { area = "Admin" });
            }

            Response.StatusCode = 400;
            return PartialView("_AddVoucherPartial", model);
        }

        public ActionResult GetVoucherTable(string searchCode, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var vouchers = db.Vouchers.AsQueryable();

            if (!string.IsNullOrEmpty(searchCode))
            {
                string lowered = searchCode.ToLower();
                vouchers = vouchers.Where(e => e.Code.ToLower().Contains(lowered));
            }

            ViewBag.CurrentSearch = searchCode;

            var paged = vouchers.OrderBy(e => e.Id).ToPagedList(pageNumber, pageSize);
            return PartialView("_VoucherTablePartial", paged);
        }

        public ActionResult EditPartial(long id)
        {
            var voucher = db.Vouchers.Find(id);
            if (voucher == null) return HttpNotFound();

            var model = new EditVoucherViewModel
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Discount = voucher.Discount,
                Quantity = voucher.Quantity,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                Available = voucher.Available
            };

            return PartialView("_EditVoucherPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditVoucherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("_EditVoucherPartial", model);
            }

            var voucher = db.Vouchers.Find(model.Id);
            if (voucher == null) return HttpNotFound();

            bool codeExists = db.Vouchers.Any(v => v.Code == model.Code && v.Id != model.Id);
            if (codeExists)
            {
                ModelState.AddModelError("Code", "Mã voucher đã tồn tại.");
                Response.StatusCode = 400;
                return PartialView("_EditVoucherPartial", model);
            }

            voucher.Code = model.Code;
            voucher.Discount = model.Discount;
            voucher.Quantity = model.Quantity;
            voucher.StartDate = model.StartDate;
            voucher.EndDate = model.EndDate;
            voucher.Available = model.Available;

            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteSelected(List<long> ids)
        {
            if (ids == null || !ids.Any())
                return new HttpStatusCodeResult(400, "Không có voucher nào được chọn.");

            var vouchers = db.Vouchers.Where(v => ids.Contains(v.Id)).ToList();

            db.Vouchers.RemoveRange(vouchers);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Delete(long id)
        {
            var voucher = db.Vouchers.Find(id);
            if (voucher == null)
                return HttpNotFound();

            db.Vouchers.Remove(voucher);
            db.SaveChanges();

            return Json(new { success = true });
        }

        public ActionResult Export(string format)
        {
            var vouchers = db.Vouchers.OrderBy(v => v.Id).ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            switch (format.ToLower())
            {
                case "csv":
                    var csv = new StringBuilder();
                    csv.AppendLine("Id,Code,Discount,Quantity,StartDate,EndDate,Available");
                    foreach (var v in vouchers)
                    {
                        csv.AppendLine($"{v.Id},\"{v.Code}\",{v.Discount},{v.Quantity},{v.StartDate:yyyy-MM-dd HH:mm},{v.EndDate:yyyy-MM-dd HH:mm},\"{v.Available}\"");
                    }

                    byte[] bufferCsv = Encoding.UTF8.GetBytes(csv.ToString());
                    return File(bufferCsv, "text/csv", "vouchers.csv");

                case "txt":
                    var lines = new List<string> { "Id\tCode\tDiscount\tQuantity\tStartDate\tEndDate\tAvailable" };
                    lines.AddRange(vouchers.Select(v =>
                        $"{v.Id}\t{v.Code}\t{v.Discount}\t{v.Quantity}\t{v.StartDate:yyyy-MM-dd HH:mm}\t{v.EndDate:yyyy-MM-dd HH:mm}\t{v.Available}"));
                    byte[] bufferTxt = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
                    return File(bufferTxt, "text/plain", "vouchers.txt");

                case "xlsx":
                default:
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("Vouchers");
                        ws.Cells[1, 1].Value = "Id";
                        ws.Cells[1, 2].Value = "Code";
                        ws.Cells[1, 3].Value = "Discount";
                        ws.Cells[1, 4].Value = "Quantity";
                        ws.Cells[1, 5].Value = "StartDate";
                        ws.Cells[1, 6].Value = "EndDate";
                        ws.Cells[1, 7].Value = "Available";

                        for (int i = 0; i < vouchers.Count; i++)
                        {
                            var v = vouchers[i];
                            ws.Cells[i + 2, 1].Value = v.Id;
                            ws.Cells[i + 2, 2].Value = v.Code;
                            ws.Cells[i + 2, 3].Value = v.Discount;
                            ws.Cells[i + 2, 4].Value = v.Quantity;
                            ws.Cells[i + 2, 5].Value = v.StartDate.ToString("yyyy-MM-dd HH:mm");
                            ws.Cells[i + 2, 6].Value = v.EndDate.ToString("yyyy-MM-dd HH:mm");
                            ws.Cells[i + 2, 7].Value = v.Available;
                        }

                        var file = package.GetAsByteArray();
                        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "vouchers.xlsx");
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
                if (ext == ".xlsx")
                {
                    using (var package = new ExcelPackage(importFile.InputStream))
                    {
                        var ws = package.Workbook.Worksheets.FirstOrDefault();
                        if (ws == null) throw new Exception("File Excel không hợp lệ.");

                        int rowCount = ws.Dimension.End.Row;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            string code = ws.Cells[row, 2].Text?.Trim();
                            decimal discount = decimal.TryParse(ws.Cells[row, 3].Text, out var d) ? d : 0;
                            int quantity = int.TryParse(ws.Cells[row, 4].Text, out var q) ? q : 0;
                            DateTime.TryParse(ws.Cells[row, 5].Text, out var start);
                            DateTime.TryParse(ws.Cells[row, 6].Text, out var end);
                            string available = ws.Cells[row, 7].Text?.Trim();

                            if (string.IsNullOrWhiteSpace(code)) continue;

                            db.Vouchers.Add(new Voucher
                            {
                                Code = code,
                                Discount = discount,
                                Quantity = quantity,
                                StartDate = start,
                                EndDate = end,
                                Available = available ?? "Active"
                            });
                        }
                    }
                }
                else if (ext == ".csv" || ext == ".txt")
                {
                    using (var reader = new StreamReader(importFile.InputStream))
                    {
                        bool isHeader = true;
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (isHeader) { isHeader = false; continue; }

                            var values = ext == ".csv" ? SplitCsvLine(line) : line.Split('\t');
                            if (values.Length < 7) continue;

                            string code = values[1].Trim();
                            decimal.TryParse(values[2], out var discount);
                            int.TryParse(values[3], out var quantity);
                            DateTime.TryParse(values[4], out var start);
                            DateTime.TryParse(values[5], out var end);
                            string available = values[6].Trim();

                            if (string.IsNullOrWhiteSpace(code)) continue;

                            db.Vouchers.Add(new Voucher
                            {
                                Code = code,
                                Discount = discount,
                                Quantity = quantity,
                                StartDate = start,
                                EndDate = end,
                                Available = available ?? "Active"
                            });
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Định dạng file không hỗ trợ.";
                    return RedirectToAction("Index");
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Import thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Import lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private string[] SplitCsvLine(string line)
        {
            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
            return System.Text.RegularExpressions.Regex.Split(line, pattern);
        }
    }
}