using EPR.Calculator.FSS.API.Common.Validators;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.Calculator.FSS.API.Common.UnitTests.Validators
{
    /// <summary>
    /// Implementation of <see cref="AbstractValidator{T}"/> for unit testing purposes.
    /// </summary>
    /// <inheritdoc/>
    public class MockRunIdValidator(Func<bool> returnValueGenerator) : RunIdValidator
    {
        private Func<bool> ReturnValueGenerator { get; init; } = returnValueGenerator;

        /// <inheritdoc/>
        public override ValidationResult Validate(ValidationContext<int> context)
        {
            if (!this.ReturnValueGenerator.Invoke())
            {
                return new ValidationResult([new ValidationFailure()]);
            }

            return new ValidationResult();
        }
    }
}