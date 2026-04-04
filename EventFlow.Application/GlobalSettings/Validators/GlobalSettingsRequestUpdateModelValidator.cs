using EventFlow.Application.GlobalSettings.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.GlobalSettings.Validators
{
    public class GlobalSettingsRequestUpdateModelValidator : AbstractValidator<GlobalSettingsRequestUpdateModel>
    {
        public GlobalSettingsRequestUpdateModelValidator()
        {
            RuleFor(x => x.Value)
               .GreaterThanOrEqualTo(0).WithMessage("Value of global settings must be greater than or equal to 0");
        }
    }
}
