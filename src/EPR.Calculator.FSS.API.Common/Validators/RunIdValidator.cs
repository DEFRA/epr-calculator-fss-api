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
                .NotEmpty()
                .WithMessage("Run ID cannot be empty.")
                .GreaterThan(0)
                .WithMessage("Run ID must be greater than 0.");
        }
    }
}