using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SaleOnline.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Cart()
        {
            var user = Session["User"] as UserAccount;
            if (user == null) return RedirectToAction("Login", "Account");

            using (var db = new SaleOnlineEntities())
            {
                var cart = db.Carts.FirstOrDefault(c => c.UserId == user.Id);
                if (cart == null) return View(new List<CartItemViewModel>());

                var items = (from ci in db.CartItems
                             join vo in db.VariationOptions on ci.VariationOptionId equals vo.Id
                             join v in db.Variations on vo.VariationId equals v.Id
                             join p in db.Products on v.ProductId equals p.Id
                             where ci.CartId == cart.Id
                             select new CartItemViewModel
                             {
                                 CartItemId = ci.Id,
                                 ProductName = p.Name,
                                 VariationOptionName = vo.Value,
                                 ImageUrl = vo.Image,
                                 FinalPrice = vo.FinalPrice,
                                 Quantity = ci.Quantity,
                                 Stock = vo.Stock
                             }).ToList();

                return View(items);
            }
        }

        [HttpPost]
        public JsonResult DeleteCartItem(long id)
        {
            try
            {
                using (var db = new SaleOnlineEntities())
                {
                    var item = db.CartItems.Find(id);
                    if (item == null)
                        return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

                    db.CartItems.Remove(item);
                    db.SaveChanges();

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteMultipleCartItems(List<long> ids)
        {
            try
            {
                using (var db = new SaleOnlineEntities())
                {
                    var items = db.CartItems.Where(ci => ids.Contains(ci.Id)).ToList();
                    db.CartItems.RemoveRange(items);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public JsonResult UpdateCartItemQuantity(long id, int quantity)
        {
            try
            {
                using (var db = new SaleOnlineEntities())
                {
                    var item = db.CartItems.Find(id);
                    if (item == null) return Json(false);

                    int stock = (int)item.VariationOption.Stock;
                    item.Quantity = Math.Min(quantity, stock);
                    db.SaveChanges();
                    return Json(true);
                }
            }
            catch
            {
                return Json(false);
            }
        }
    }
}