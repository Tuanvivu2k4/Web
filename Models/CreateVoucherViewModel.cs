using System;
using System.ComponentModel.DataAnnotations;
using SaleOnline.Areas.Admin.DTO;

namespace SaleOnline.Models
{
    public class CreateVoucherViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã voucher")]
        [StringLength(50, ErrorMessage = "Mã không được vượt quá 50 ký tự")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm giá")]
        [Range(0, 100, ErrorMessage = "Giảm giá phải từ 0 đến 100")]
        public decimal Discount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        [DateGreaterThan("StartDate", ErrorMessage = "Ngày kết thúc phải sau ngày bắt đầu")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string Available { get; set; }
    }
}