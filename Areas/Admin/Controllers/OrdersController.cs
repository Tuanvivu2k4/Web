using PagedList;
using SaleOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using OfficeOpenXml;
using System.Text;
using System.Net;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class OrdersController : BaseAdminController
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();
        // GET: Admin/Orders
        public ActionResult Index(long? id, string paymentMethod, string status, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var orders = db.Orders
                .Include(o => o.UserAccount)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    CustomerName = o.UserAccount.Name,
                    PhoneNumber = o.PhoneNumber,
                    Address = o.Address,
                    OrderDate = o.OrderDate,
                    PaymentMethod = o.PaymentMethod,
                    FinalPrice = o.FinalPrice,
                    Status = o.Status
                });

            if (id.HasValue)
                orders = orders.Where(o => o.Id == id.Value);

            if (!string.IsNullOrEmpty(paymentMethod))
                orders = orders.Where(o => o.PaymentMethod == paymentMethod);

            if (!string.IsNullOrEmpty(status))
                orders = orders.Where(o => o.Status == status);

            ViewBag.CurrentId = id;
            ViewBag.CurrentPaymentMethod = paymentMethod;
            ViewBag.CurrentStatus = status;

            var paged = orders.OrderByDescending(o => o.OrderDate).ToPagedList(pageNumber, pageSize);
            return View(paged);
        }

        public ActionResult GetOrderTable(long? id, string paymentMethod, string status, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var orders = db.Orders
                .Include(o => o.UserAccount)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    CustomerName = o.UserAccount.Name,
                    PhoneNumber = o.PhoneNumber,
                    Address = o.Address,
                    OrderDate = o.OrderDate,
                    PaymentMethod = o.PaymentMethod,
                    FinalPrice = o.FinalPrice,
                    Status = o.Status
                });

            if (id.HasValue)
                orders = orders.Where(o => o.Id == id.Value);

            if (!string.IsNullOrEmpty(paymentMethod))
                orders = orders.Where(o => o.PaymentMethod == paymentMethod);

            if (!string.IsNullOrEmpty(status))
                orders = orders.Where(o => o.Status == status);

            ViewBag.CurrentId = id;
            ViewBag.CurrentPaymentMethod = paymentMethod;
            ViewBag.CurrentStatus = status;

            var paged = orders.OrderByDescending(o => o.OrderDate).ToPagedList(pageNumber, pageSize);
            return PartialView("_OrderTablePartial", paged);
        }
        public ActionResult DetailPartial(long id)
        {
            var order = db.Orders
                .Include(o => o.UserAccount)
                .Include(o => o.OrderItems.Select(oi => oi.VariationOption.Variation.Product))
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return HttpNotFound();

            var model = new OrderDetailViewModel
            {
                Id = order.Id,
                CustomerName = order.UserAccount.Name,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                PaymentMethod = order.PaymentMethod,
                OrderDate = order.OrderDate,
                FinalPrice = order.FinalPrice,
                Status = order.Status,
                VoucherCode = order.VoucherCode,
                Discount = order.Discount,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.VariationOption.Variation.Product.Name,
                    VariationOptionValue = oi.VariationOption.Value,
                    Quantity = oi.Quantity,
                    PriceItem = oi.PriceItem
                }).ToList()
            };

            return PartialView("_OrderDetailPartial", model);
        }

        [HttpGet]
        public ActionResult Export(string format)
        {
            var orders = db.Orders
                .Include(o => o.UserAccount)
                .Include(o => o.OrderItems.Select(oi => oi.VariationOption.Variation.Product))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            switch (format.ToLower())
            {
                case "csv":
                    var csv = new StringBuilder();
                    csv.AppendLine("OrderId,CustomerName,Phone,Address,OrderDate,PaymentMethod,Status,FinalPrice,ProductName,OptionValue,Quantity,PriceItem");

                    foreach (var o in orders)
                    {
                        foreach (var item in o.OrderItems)
                        {
                            csv.AppendLine($"{o.Id},\"{o.UserAccount?.Name}\",\"{o.PhoneNumber}\",\"{o.Address}\",{o.OrderDate:yyyy-MM-dd HH:mm},\"{o.PaymentMethod}\",\"{o.Status}\",{o.FinalPrice},\"{item.VariationOption.Variation.Product.Name}\",\"{item.VariationOption.Value}\",{item.Quantity},{item.PriceItem}");
                        }
                    }

                    byte[] csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
                    return File(csvBytes, "text/csv", "orders.csv");

                case "txt":
                    var lines = new List<string> { "OrderId\tCustomerName\tPhone\tAddress\tOrderDate\tPaymentMethod\tStatus\tFinalPrice\tProductName\tOptionValue\tQuantity\tPriceItem" };

                    foreach (var o in orders)
                    {
                        foreach (var item in o.OrderItems)
                        {
                            lines.Add($"{o.Id}\t{o.UserAccount?.Name}\t{o.PhoneNumber}\t{o.Address}\t{o.OrderDate:yyyy-MM-dd HH:mm}\t{o.PaymentMethod}\t{o.Status}\t{o.FinalPrice}\t{item.VariationOption.Variation.Product.Name}\t{item.VariationOption.Value}\t{item.Quantity}\t{item.PriceItem}");
                        }
                    }

                    byte[] txtBytes = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
                    return File(txtBytes, "text/plain", "orders.txt");

                case "xlsx":
                default:
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("Orders");

                        ws.Cells[1, 1].Value = "OrderId";
                        ws.Cells[1, 2].Value = "CustomerName";
                        ws.Cells[1, 3].Value = "Phone";
                        ws.Cells[1, 4].Value = "Address";
                        ws.Cells[1, 5].Value = "OrderDate";
                        ws.Cells[1, 6].Value = "PaymentMethod";
                        ws.Cells[1, 7].Value = "Status";
                        ws.Cells[1, 8].Value = "FinalPrice";
                        ws.Cells[1, 9].Value = "ProductName";
                        ws.Cells[1, 10].Value = "OptionValue";
                        ws.Cells[1, 11].Value = "Quantity";
                        ws.Cells[1, 12].Value = "PriceItem";

                        int row = 2;
                        foreach (var o in orders)
                        {
                            foreach (var item in o.OrderItems)
                            {
                                ws.Cells[row, 1].Value = o.Id;
                                ws.Cells[row, 2].Value = o.UserAccount?.Name;
                                ws.Cells[row, 3].Value = o.PhoneNumber;
                                ws.Cells[row, 4].Value = o.Address;
                                ws.Cells[row, 5].Value = o.OrderDate.ToString("yyyy-MM-dd HH:mm");
                                ws.Cells[row, 6].Value = o.PaymentMethod;
                                ws.Cells[row, 7].Value = o.Status;
                                ws.Cells[row, 8].Value = o.FinalPrice;
                                ws.Cells[row, 9].Value = item.VariationOption.Variation.Product.Name;
                                ws.Cells[row, 10].Value = item.VariationOption.Value;
                                ws.Cells[row, 11].Value = item.Quantity;
                                ws.Cells[row, 12].Value = item.PriceItem;
                                row++;
                            }
                        }

                        var file = package.GetAsByteArray();
                        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
                    }
            }
        }

        public ActionResult EditStatus(long id)
        {
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            var model = new UpdateOrderStatusViewModel
            {
                Id = order.Id,
                Status = order.Status
            };

            return PartialView("_EditOrderStatusPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(UpdateOrderStatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("_EditStatusPartial", model);
            }

            var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.VariationOption.Variation.Product))
                                 .FirstOrDefault(o => o.Id == model.Id);

            if (order == null) return HttpNotFound();

            var oldStatus = order.Status;
            order.Status = model.Status;
            db.Entry(order).State = EntityState.Modified;

            // Nếu chuyển từ khác sang "Đã giao hàng", cộng QuantitySold
            if (oldStatus != "Đã giao hàng" && model.Status == "Đã giao hàng")
            {
                foreach (var item in order.OrderItems)
                {
                    var product = item.VariationOption.Variation.Product;
                    product.QuantitySold += item.Quantity;
                }
            }
            // Nếu chuyển từ "Đã giao hàng" sang trạng thái khác, trừ QuantitySold
            else if (oldStatus == "Đã giao hàng" && model.Status != "Đã giao hàng")
            {
                foreach (var item in order.OrderItems)
                {
                    var product = item.VariationOption.Variation.Product;
                    product.QuantitySold = Math.Max(0, product.QuantitySold - item.Quantity);
                }
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        public ActionResult EditMultipleStatus(List<long> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new HttpStatusCodeResult(400, "Không có đơn hàng nào được chọn.");
            }

            var model = new UpdateMultipleOrdersStatusViewModel
            {
                Ids = ids,
                Status = ""
            };

            return PartialView("_EditMultipleOrderStatusPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateMultipleStatus(List<long> ids, string status)
        {
            if (ids == null || !ids.Any() || string.IsNullOrEmpty(status))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var orders = db.Orders
                .Where(o => ids.Contains(o.Id))
                .Include(o => o.OrderItems.Select(oi => oi.VariationOption.Variation.Product))
                .ToList();

            foreach (var order in orders)
            {
                var oldStatus = order.Status;

                if (oldStatus != status)
                {
                    // Cập nhật QuantitySold nếu chuyển sang "Đã giao hàng"
                    if (status == "Đã giao hàng" && oldStatus != "Đã giao hàng")
                    {
                        foreach (var item in order.OrderItems)
                        {
                            var product = item.VariationOption.Variation.Product;
                            product.QuantitySold += item.Quantity;
                        }
                    }
                    // Giảm QuantitySold nếu chuyển từ "Đã giao hàng" sang trạng thái khác
                    else if (oldStatus == "Đã giao hàng" && status != "Đã giao hàng")
                    {
                        foreach (var item in order.OrderItems)
                        {
                            var product = item.VariationOption.Variation.Product;
                            product.QuantitySold = Math.Max(0, product.QuantitySold - item.Quantity);
                        }
                    }

                    order.Status = status;
                    db.Entry(order).State = EntityState.Modified;
                }
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        public ActionResult Print(long id)
        {
            var order = db.Orders
                .Include(o => o.UserAccount)
                .Include(o => o.OrderItems.Select(oi => oi.VariationOption.Variation.Product))
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return HttpNotFound();

            // Tính số tiền cần thanh toán
            decimal amountToPay = order.PaymentMethod == "Paid" ? 0 : order.FinalPrice;

            var viewModel = new OrderPrintViewModel
            {
                Id = order.Id,
                CustomerName = order.UserAccount?.Name,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                OrderDate = order.OrderDate,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new OrderPrintItem
                {
                    ProductName = oi.VariationOption.Variation.Product.Name,
                    Variation = oi.VariationOption.Value,
                    Quantity = oi.Quantity,
                    Price = oi.PriceItem
                }).ToList(),
                Total = order.FinalPrice,
                AmountToPay = amountToPay
            };

            return View("PrintOrder", viewModel);
        }
    }
}