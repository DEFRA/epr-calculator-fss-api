using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            RuleFor(runId => runId)
                .NotEmpty()
                .WithMessage("Run ID cannot be empty.")
                .GreaterThan(0)
                .WithMessage("Run ID must be greater than 0.");
        }
    }
}
