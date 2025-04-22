using System.ComponentModel.DataAnnotations;

namespace SaleOnline.Models
{
    public class EditCustomerStatusViewModel
    {
        [Required]
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái tài khoản.")]
        public string AccountStatus { get; set; }
    }
}
