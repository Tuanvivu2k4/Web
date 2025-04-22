using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SaleOnline.Models
{
    public class CreateProductViewModel
    {
        // ---------- Product ----------
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày sản xuất")]
        [DataType(DataType.Date)]
        public DateTime ProductionDate { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ảnh sản phẩm")]
        public HttpPostedFileBase ImageFile { get; set; } // để upload ảnh

        public long QuantitySold { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public long CategoryId { get; set; }

        [Required]
        public string Available { get; set; }

        // ---------- Variation ----------
        public string VariationName { get; set; } // Có thể null

        [Required(ErrorMessage = "Vui lòng chọn trạng thái của biến thể")]
        public string VariationAvailable { get; set; }

        // ---------- VariationOptions ----------
        public List<VariationOptionViewModel> Options { get; set; }
    }

    public class VariationOptionViewModel
    {
        public string Value { get; set; } // Có thể null

        [Required(ErrorMessage = "Ảnh không được để trống")]
        public HttpPostedFileBase ImageFile { get; set; } // để upload riêng mỗi option

        [Range(0, long.MaxValue, ErrorMessage = "Tồn kho phải >= 0")]
        public long Stock { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OriginalPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }

        [Range(0, 1)]
        public decimal Discount { get; set; }

        public decimal FinalPrice { get; set; }

        [Required]
        public string Available { get; set; }
    }
}
