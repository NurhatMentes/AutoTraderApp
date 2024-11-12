using FluentValidation.Results;

namespace AutoTraderApp.Core.CrossCuttingConcerns.Validation
{
    public class ValidationException : Exception
    {
        public IEnumerable<ValidationFailure> Errors { get; }

        public ValidationException(IEnumerable<ValidationFailure> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
