using System;

namespace SaleOnline.Areas.Admin.DTO
{
    public class DashboardDTO
    {
        public decimal TodayOrders { get; set; }
        public decimal YesterdayOrders { get; set; }
        public decimal ThisMonth { get; set; }
        public decimal LastMonth { get; set; }
        public decimal TotalSales { get; set; }

        public int TotalOrders { get; set; }
        public int OrdersPending { get; set; }
        public int OrdersProcessing { get; set; }
        public int OrdersDelivered { get; set; }

        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalProducts { get; set; }
        public int TotalProductTypes { get; set; }
    }

    public class BestSellingProductDTO
    {
        public string Name { get; set; }
        public int QuantitySold { get; set; }
    }

    public class WeeklySalesDTO
    {
        public DateTime Date { get; set; }
        public decimal Sales { get; set; }
    }
}
