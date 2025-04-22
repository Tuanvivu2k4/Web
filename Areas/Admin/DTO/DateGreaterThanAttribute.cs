using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SaleOnline.Areas.Admin.DTO
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public DateGreaterThanAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var thisDate = (DateTime?)value;
            var property = validationContext.ObjectType.GetProperty(_otherProperty);
            var otherDate = (DateTime?)property?.GetValue(validationContext.ObjectInstance);

            if (thisDate.HasValue && otherDate.HasValue && thisDate <= otherDate)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}