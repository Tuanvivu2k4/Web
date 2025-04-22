using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SaleOnline.Models
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(255)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ảnh")]
        public HttpPostedFileBase ImageFile { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string Available { get; set; }
    }
}