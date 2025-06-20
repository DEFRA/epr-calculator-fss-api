using EPR.Calculator.FSS.API.Common.Properties;
using FluentValidation;

namespace EPR.Calculator.FSS.API.Common.Validators
{
    /// <summary>
    /// Validator for Run IDs.
    /// </summary>
    public class RunIdValidator : AbstractValidator<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunIdValidator"/> class.
        /// </summary>
        public RunIdValidator()
        {
            this.RuleFor(runId => runId)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage(Resources.RunIdIsEmpty)
                .GreaterThan(0)
                .WithMessage(Resources.RunIdIsZero);
        }
    }
}