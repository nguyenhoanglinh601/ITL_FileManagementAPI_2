using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementAPI.Infrastructure.Extention.CustomerValidation
{
    public class CustomerValidation
    {
        public static ValidationResult WhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value)
                ? new ValidationResult("The value isn't valid")
                : ValidationResult.Success;
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class WhiteSpaceValidationAttribute : ValidationAttribute
    {
        //public string _value { get; set; }
        public WhiteSpaceValidationAttribute()
        {

        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;
            return ((string)value).Trim() == string.Empty
              ? new ValidationResult(ErrorMessageString)
              : ValidationResult.Success;

        }
    }
}
