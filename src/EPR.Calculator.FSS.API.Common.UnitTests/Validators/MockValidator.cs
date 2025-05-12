using FluentValidation;
using FluentValidation.Results;

namespace EPR.Calculator.FSS.API.Common.UnitTests.Validators
{
    /// <summary>
    /// Implementation of <see cref="AbstractValidator{T}"/> for unit testing purposes.
    /// </summary>
    /// <inheritdoc/>
    public class MockValidator<T> : AbstractValidator<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockValidator{T}"/> class.
        /// </summary>
        /// <param name="returnValueGenerator">
        /// A function that returns true or false
        /// - used as the return value when validating.
        /// </param>
        public MockValidator(Func<bool> returnValueGenerator) => this.ReturnValueGenerator = returnValueGenerator;

        private Func<bool> ReturnValueGenerator { get; init; }

        /// <inheritdoc/>
        public override ValidationResult Validate(ValidationContext<T> context)
        {
            if (!this.ReturnValueGenerator.Invoke())
            {
                return new ValidationResult([new ValidationFailure()]);
            }

            return new ValidationResult();
        }
    }
}
