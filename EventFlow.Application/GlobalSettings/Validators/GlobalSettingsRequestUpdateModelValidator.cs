using EventFlow.Application.GlobalSettings.Requests;
using EventFlow.Application.Localization;
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
               .GreaterThanOrEqualTo(0).WithMessage(x => ValidationMessages.GlobalSettingValueMinimum);
        }
    }
}
