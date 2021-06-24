using ITL.NetCore.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FileManagement.DL.Annotations
{
    class PhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (Utility.IsPhone(value.ToString(),true))
            {
                return ValidationResult.Success;
            }
            var msg = $"Please enter value valid Phonenumber";
            return new ValidationResult(msg);
        }

    }
}
