using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace SaleOnline.Models
{
    public class EditProductViewModel
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ProductionDate { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }

        [Required]
        public long CategoryId { get; set; }

        public long QuantitySold { get; set; }

        [Required]
        public string Available { get; set; }

        public string VariationName { get; set; }
        public string VariationAvailable { get; set; }

        public List<VariationOptionViewModel1> Options { get; set; }
    }

    public class VariationOptionViewModel1
    {
        public long? Id { get; set; } // dùng để biết đang sửa cái nào
        public string Value { get; set; }
        public string Image { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }

        public long Stock { get; set; }

        public decimal OriginalPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }

        public string Available { get; set; }
    }
}