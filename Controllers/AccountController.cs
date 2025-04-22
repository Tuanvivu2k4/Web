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
        public ActionResult Register()
        {
            if (Session["UserId"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (Session["UserId"] != null)
                return RedirectToAction("Index", "Home");
            if (ModelState.IsValid)
            {
                if (db.UserAccounts.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được đăng ký");
                    return View(model);
                }

                var user = new UserAccount
                {
                    Email = model.Email,
                    Password = PasswordHelper.HashPassword(model.Password),
                    Name = model.Name,
                    DateOfBirth = model.DateOfBirth,
                    AccountStatus = "Active"
                };

                db.UserAccounts.Add(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đăng ký thành công!";
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (Session["UserId"] != null)
                return RedirectToAction("Index", "Home");
            if (ModelState.IsValid)
            {
                var user = db.UserAccounts.FirstOrDefault(u => u.Email == model.Email);

                if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.Password))
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                    return View(model);
                }

                Session["UserId"] = user.Id;
                Session["UserName"] = user.Name;
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}