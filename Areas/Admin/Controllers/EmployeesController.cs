using OfficeOpenXml;
using PagedList;
using SaleOnline.Models;
using SaleOnline.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class EmployeesController : BaseAdminController
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();
        // GET: Admin/Employees
        public ActionResult Index(string search, string role, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var employees = db.Admins.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                string lowered = search.ToLower();
                employees = employees.Where(e => e.Name.ToLower().Contains(lowered));
            }

            if (!string.IsNullOrEmpty(role) && role != "All")
            {
                employees = employees.Where(e => e.Role == role);
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;

            var paged = employees.OrderBy(e => e.Id).ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool accountExists = db.Admins.Any(a => a.Account == model.Account);
                if (accountExists)
                {
                    ModelState.AddModelError("Account", "Tài khoản này đã tồn tại.");
                    Response.StatusCode = 400;
                    return PartialView("_AddEmployeePartial", model);
                }

                var employee = new SaleOnline.Models.Admin
                {
                    Account = model.Account,
                    Password = PasswordHelper.HashPassword(model.Password),
                    Name = model.Name,
                    Role = model.Role
                };

                db.Admins.Add(employee);
                db.SaveChanges();
                return RedirectToAction("Index", "Employees", new { area = "Admin" });
            }

            Response.StatusCode = 400;
            return PartialView("_AddEmployeePartial", model);
        }
        public ActionResult GetEmployeeTable(string search, string role, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var employees = db.Admins.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                string lowered = search.ToLower();
                employees = employees.Where(e => e.Name.ToLower().Contains(lowered));
            }

            if (!string.IsNullOrEmpty(role) && role != "All")
            {
                employees = employees.Where(e => e.Role == role);
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;

            var paged = employees.OrderBy(e => e.Id).ToPagedList(pageNumber, pageSize);
            return PartialView("_EmployeeTablePartial", employees);
        }

        public ActionResult Export(string format)
        {
            var employees = db.Admins.OrderBy(e => e.Id).ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            switch (format.ToLower())
            {
                case "csv":
                    var csv = new StringBuilder();
                    csv.AppendLine("Id,Account,Name,Role");
                    foreach (var emp in employees)
                    {
                        csv.AppendLine($"{emp.Id},\"{emp.Account}\",\"{emp.Name}\",\"{emp.Role}\"");
                    }
                    byte[] bufferCsv = Encoding.UTF8.GetBytes(csv.ToString());
                    return File(bufferCsv, "text/csv", "employees.csv");

                case "txt":
                    var lines = new List<string> { "Id\tAccount\tName\tRole" };
                    lines.AddRange(employees.Select(e => $"{e.Id}\t{e.Account}\t{e.Name}\t{e.Role}"));
                    byte[] bufferTxt = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
                    return File(bufferTxt, "text/plain", "employees.txt");

                case "xlsx":
                default:
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("Employees");
                        ws.Cells[1, 1].Value = "Id";
                        ws.Cells[1, 2].Value = "Account";
                        ws.Cells[1, 3].Value = "Name";
                        ws.Cells[1, 4].Value = "Role";

                        for (int i = 0; i < employees.Count; i++)
                        {
                            var e = employees[i];
                            ws.Cells[i + 2, 1].Value = e.Id;
                            ws.Cells[i + 2, 2].Value = e.Account;
                            ws.Cells[i + 2, 3].Value = e.Name;
                            ws.Cells[i + 2, 4].Value = e.Role;
                        }

                        var file = package.GetAsByteArray();
                        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "employees.xlsx");
                    }
            }
        }

        [HttpPost]
        public ActionResult DeleteSelected(List<long> ids)
        {
            var currentAdmin = Session["Admin"] as SaleOnline.Models.Admin;
            if (currentAdmin == null)
                return new HttpStatusCodeResult(403, "Phiên làm việc đã hết hạn.");

            if (ids == null || !ids.Any())
                return new HttpStatusCodeResult(400, "Không có nhân viên nào được chọn.");

            foreach (var id in ids)
            {
                if (id == currentAdmin.Id) continue; // bỏ qua chính mình

                var emp = db.Admins.Find(id);
                if (emp != null)
                {
                    db.Admins.Remove(emp);
                }
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Delete(long id)
        {
            var currentAdmin = Session["Admin"] as SaleOnline.Models.Admin;
            if (currentAdmin == null)
                return new HttpStatusCodeResult(403, "Phiên làm việc đã hết hạn.");

            var emp = db.Admins.Find(id);
            if (emp == null)
                return HttpNotFound();

            // Không cho phép xoá chính mình
            if (emp.Id == currentAdmin.Id)
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = "Bạn không thể xoá tài khoản đang đăng nhập." });
            }

            db.Admins.Remove(emp);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ResetPassword(long id)
        {
            var emp = db.Admins.Find(id);
            if (emp == null)
                return HttpNotFound();

            // Reset về mật khẩu mặc định "123456"
            emp.Password = PasswordHelper.HashPassword("123456");
            db.SaveChanges();

            return Json(new { success = true });
        }

        public ActionResult EditPartial(long id)
        {
            var emp = db.Admins.Find(id);
            if (emp == null) return HttpNotFound();

            var model = new EditAdminViewModel
            {
                Id = emp.Id,
                Account = emp.Account,
                Name = emp.Name,
                Role = emp.Role
            };

            return PartialView("_EditEmployeePartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("_EditEmployeePartial", model);
            }

            var emp = db.Admins.Find(model.Id);
            if (emp == null) return HttpNotFound();

            bool accountExists = db.Admins.Any(a => a.Account == model.Account && a.Id != model.Id);
            if (accountExists)
            {
                ModelState.AddModelError("Account", "Tài khoản này đã tồn tại.");
                Response.StatusCode = 400;
                return PartialView("_EditEmployeePartial", model);
            }

            emp.Account = model.Account;
            emp.Name = model.Name;
            emp.Role = model.Role;

            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}