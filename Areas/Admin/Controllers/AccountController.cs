using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SaleOnline.Models;
using SaleOnline.Utils;
using SaleOnline.ViewModels;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();

        // GET: Admin/Account/Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Tìm tài khoản theo email hoặc account
            var admin = db.Admins.FirstOrDefault(a => a.Account == model.Account);

            // Kiểm tra nếu tài khoản tồn tại và mật khẩu đúng
            if (admin != null && PasswordHelper.VerifyPassword(model.Password, admin.Password))
            {
                // Lưu thông tin vào Session
                Session["Admin"] = admin;

                // Chuyển hướng theo Role
                if (admin.Role.ToLower() == "admin")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (admin.Role.ToLower() == "employee")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Employee" });
                }
                else
                {
                    ModelState.AddModelError("", "Quyền không hợp lệ.");
                    return View(model);
                }
            }

            ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
            return View(model);
        }

        // GET: Admin/Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Admins.FirstOrDefault(u => u.Account == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                return View(model);
            }

            TempData["Message"] = "Mật khẩu đã được gửi đến email của bạn (giả lập).";
            return RedirectToAction("Login");
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = (SaleOnline.Models.Admin)Session["Admin"];
            var dbAdmin = db.Admins.Find(admin.Id);

            if (!PasswordHelper.VerifyPassword(model.CurrentPassword, dbAdmin.Password))
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            dbAdmin.Password = PasswordHelper.HashPassword(model.NewPassword);
            db.SaveChanges();

            TempData["Message"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
