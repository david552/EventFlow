using EventFlow.Application.GlobalSettings.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.GlobalSettings.Validators
{
    public class GlobalSettingsRequestCreateModelValidator : AbstractValidator<GlobalSettingsRequestCreateModel>
    {
        public GlobalSettingsRequestCreateModelValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Value of global settings must be greater than or equal to 0");
            RuleFor(x => x.Key)
               .NotEmpty().WithMessage("Global settings Key cannot be empty")
               .MaximumLength(30).WithMessage("Global settings Key cannot contain more than 30 characters");
               


        }
    }
}
