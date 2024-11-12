using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace AutoTraderApp.Core.CrossCuttingConcerns.Validation
{
    public static class ValidationTool
    {
        public static void Validate(IValidator validator, object entity)
        {
            var context = new ValidationContext<object>(entity);
            var result = validator.Validate(context);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        public static void Validate<T>(IValidator<T> validator, T entity)
        {
            var context = new ValidationContext<T>(entity);
            var result = validator.Validate(context);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        public static async Task ValidateAsync<T>(IValidator<T> validator, T entity)
        {
            var context = new ValidationContext<T>(entity);
            var result = await validator.ValidateAsync(context);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        public static ValidationResult ValidateAndGetResult<T>(IValidator<T> validator, T entity)
        {
            var context = new ValidationContext<T>(entity);
            return validator.Validate(context);
        }
    }
}
