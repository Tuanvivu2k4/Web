using System.ComponentModel.DataAnnotations;

namespace SaleOnline.Models
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        public string Account { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 50 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
