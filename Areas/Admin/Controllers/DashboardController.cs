using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using OfficeOpenXml;
using SaleOnline.Models;

namespace SaleOnline.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        private SaleOnlineEntities db = new SaleOnlineEntities();

        public ActionResult Index()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            // Doanh thu theo thời gian
            ViewBag.TodayRevenue = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) == today)
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            ViewBag.YesterdayRevenue = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) == yesterday)
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            ViewBag.ThisMonthRevenue = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) >= startOfMonth)
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            ViewBag.LastMonthRevenue = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) >= startOfLastMonth &&
                            DbFunctions.TruncateTime(o.OrderDate) <= endOfLastMonth)
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            ViewBag.TotalRevenue = db.Orders
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            // Doanh thu hôm nay theo phương thức thanh toán
            ViewBag.TodayCash = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) == today && o.PaymentMethod == "COD")
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            ViewBag.TodayPaid = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) == today && o.PaymentMethod == "Paid")
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            // Doanh thu hôm qua theo phương thức thanh toán
            ViewBag.YesterdayCash = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) == yesterday && o.PaymentMethod == "COD")
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            ViewBag.YesterdayPaid = db.Orders
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) == yesterday && o.PaymentMethod == "Paid")
                .Sum(o => (decimal?)o.FinalPrice) ?? 0;

            // Trạng thái đơn hàng
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.OrdersPending = db.Orders.Count(o => o.Status == "Chờ xác nhận");
            ViewBag.OrdersProcessing = db.Orders.Count(o => o.Status == "Đang chuẩn bị hàng");
            ViewBag.OrdersDelivered = db.Orders.Count(o => o.Status == "Đã giao hàng");

            return View();
        }

        public JsonResult GetWeeklySales()
        {
            var today = DateTime.Today;
            var days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).OrderBy(d => d).ToList();

            var sales = days.Select(d =>
            {
                var revenue = db.Orders
                    .Where(o => DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(d))
                    .Sum(o => (decimal?)o.FinalPrice) ?? 0;

                return revenue;
            }).ToList();

            return Json(new
            {
                labels = days.Select(d => d.ToString("yyyy-MM-dd")).ToArray(),
                values = sales
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBestSellingProducts()
        {
            var data = db.Products
                .Where(p => p.QuantitySold > 0)
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .Select(p => new
                {
                    Product = p.Name,
                    Quantity = p.QuantitySold
                })
                .ToList();

            return Json(new
            {
                labels = data.Select(d => d.Product).ToList(),
                values = data.Select(d => d.Quantity).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportRevenue(DateTime? startDate, DateTime? endDate, string format = "xlsx")
        {
            var orders = db.Orders
                .Where(o => o.Status == "Đã giao hàng");

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                orders = orders.Where(o => DbFunctions.TruncateTime(o.OrderDate) >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date;
                orders = orders.Where(o => DbFunctions.TruncateTime(o.OrderDate) <= end);
            }

            var list = orders
                .Include(o => o.OrderItems.Select(i => i.VariationOption))
                .ToList();

            // Tính tổng lợi nhuận
            decimal totalProfit = 0;
            var rows = list.Select(o =>
            {
                var cost = o.OrderItems.Sum(i => i.VariationOption.OriginalPrice * i.Quantity);
                var profit = o.FinalPrice - cost;
                totalProfit += profit;

                return new
                {
                    o.Id,
                    o.PhoneNumber,
                    o.Address,
                    o.OrderDate,
                    o.FinalPrice,
                    Cost = cost,
                    Profit = profit
                };
            }).ToList();

            string fileNameDate = $"{(startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "All")}_to_{(endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "All")}";
            string fileName = $"revenue-{fileNameDate}.{format}";

            if (format == "csv")
            {
                var sb = new StringBuilder();
                sb.AppendLine("Id,PhoneNumber,Address,OrderDate,FinalPrice,Cost,Profit");
                foreach (var r in rows)
                {
                    sb.AppendLine($"{r.Id},{r.PhoneNumber},{r.Address},{r.OrderDate:yyyy-MM-dd HH:mm},{r.FinalPrice},{r.Cost},{r.Profit}");
                }
                sb.AppendLine($",,,,,,Tổng lợi nhuận:,{totalProfit}");

                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Default: Excel (xlsx)
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Revenue Report");
                ws.Cells[1, 1].Value = "Id";
                ws.Cells[1, 2].Value = "PhoneNumber";
                ws.Cells[1, 3].Value = "Address";
                ws.Cells[1, 4].Value = "OrderDate";
                ws.Cells[1, 5].Value = "FinalPrice";
                ws.Cells[1, 6].Value = "Cost";
                ws.Cells[1, 7].Value = "Profit";

                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    ws.Cells[i + 2, 1].Value = r.Id;
                    ws.Cells[i + 2, 2].Value = r.PhoneNumber;
                    ws.Cells[i + 2, 3].Value = r.Address;
                    ws.Cells[i + 2, 4].Value = r.OrderDate.ToString("yyyy-MM-dd HH:mm");
                    ws.Cells[i + 2, 5].Value = r.FinalPrice;
                    ws.Cells[i + 2, 6].Value = r.Cost;
                    ws.Cells[i + 2, 7].Value = r.Profit;
                }

                ws.Cells[rows.Count + 3, 6].Value = "Tổng lợi nhuận:";
                ws.Cells[rows.Count + 3, 7].Value = totalProfit;

                var stream = package.GetAsByteArray();
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}
