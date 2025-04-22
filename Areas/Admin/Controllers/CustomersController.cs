using PagedList;
using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class CustomersController : BaseAdminController
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();

        // GET: Admin/Customers
        public ActionResult Index(string searchId, string status, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var customers = db.UserAccounts.AsQueryable();

            if (!string.IsNullOrEmpty(searchId) && long.TryParse(searchId, out long id))
            {
                customers = customers.Where(c => c.Id == id);
            }

            if (!string.IsNullOrEmpty(status))
            {
                customers = customers.Where(c => c.AccountStatus == status);
            }

            ViewBag.CurrentSearchId = searchId;
            ViewBag.CurrentStatus = status;

            var paged = customers.OrderBy(c => c.Id).ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        public ActionResult EditStatusPartial(long id)
        {
            var customer = db.UserAccounts.Find(id);
            if (customer == null) return HttpNotFound();

            var model = new EditCustomerStatusViewModel
            {
                Id = customer.Id,
                AccountStatus = customer.AccountStatus
            };

            return PartialView("_EditStatusPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStatus(EditCustomerStatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("_EditStatusPartial", model);
            }

            var customer = db.UserAccounts.Find(model.Id);
            if (customer == null) return HttpNotFound();

            customer.AccountStatus = model.AccountStatus;
            db.SaveChanges();

            return Json(new { success = true });
        }

        public ActionResult GetCustomerTable(string searchId, string status, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var customers = db.UserAccounts.AsQueryable();

            if (!string.IsNullOrEmpty(searchId) && long.TryParse(searchId, out long id))
            {
                customers = customers.Where(c => c.Id == id);
            }

            if (!string.IsNullOrEmpty(status))
            {
                customers = customers.Where(c => c.AccountStatus == status);
            }

            var paged = customers.OrderBy(c => c.Id).ToPagedList(pageNumber, pageSize);

            return PartialView("_CustomerTablePartial", paged);
        }

        public ActionResult Detail(long id)
        {
            var customer = db.UserAccounts
                .Include("UserAddresses")
                .FirstOrDefault(c => c.Id == id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }
    }
}