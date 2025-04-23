using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SaleOnline.Models
{
    public class UpdateMultipleOrdersStatusViewModel
    {
        public List<long> Ids { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        public string Status { get; set; }
    }
}