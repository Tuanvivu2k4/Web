using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SaleOnline.Models
{
    public class EditCategoryViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(255)]
        public string Name { get; set; }

        public string Image { get; set; }  // để hiển thị ảnh cũ

        public HttpPostedFileBase ImageFile { get; set; }  // để chọn ảnh mới

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string Available { get; set; }
    }
}