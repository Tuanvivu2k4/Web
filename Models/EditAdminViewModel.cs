using System.ComponentModel.DataAnnotations;

namespace SaleOnline.Models
{
    public class EditAdminViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        [StringLength(50)]
        public string Account { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn role")]
        public string Role { get; set; }
    }
}
