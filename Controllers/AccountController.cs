using SaleOnline.Models;
using SaleOnline.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SaleOnline.Controllers
{
    public class AccountController : Controller
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();
        // GET: Account
        [HttpGet]
        public ActionResult Register()
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = db.UserAccounts.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                // Gán lỗi cho đúng trường Email
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(model);
            }

            var user = new UserAccount
            {
                Name = model.Name,
                Email = model.Email,
                Password = PasswordHelper.HashPassword(model.Password),
                DateOfBirth = model.DateOfBirth,
                AccountStatus = "Active"
            };

            db.UserAccounts.Add(user);
            db.SaveChanges();

            TempData["RegisterSuccess"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = db.UserAccounts.FirstOrDefault(u => u.Email == model.Email);
            if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            Session["User"] = user;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Remove("User");
            return RedirectToAction("Index", "Home");
        }
    }
}