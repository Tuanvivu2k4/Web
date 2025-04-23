using System.ComponentModel.DataAnnotations;

namespace SaleOnline.Models
{
    public class UpdateOrderStatusViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái đơn hàng.")]
        public string Status { get; set; }
    }
}
