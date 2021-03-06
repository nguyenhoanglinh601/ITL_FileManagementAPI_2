using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CatelogueManagement.DL.Annotations
{
    class StringContainAttribute: ValidationAttribute
    {
        public string[] AllowableValues { get; set; }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Boolean finded = false;
            AllowableValues.ToList().ForEach(i =>
            {
                if (value?.ToString().ToLower().Contains(i.ToLower()) == true)
                {
                    finded = true;
                }
            });
            if (finded)
            {
                return ValidationResult.Success;
            }
            var msg = $"Please enter value contain: {string.Join(", ", (AllowableValues ?? new string[] { "No allowable values found" }))}.";
            return new ValidationResult(msg);
        }
    }
}
