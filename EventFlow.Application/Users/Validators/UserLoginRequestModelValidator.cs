using EventFlow.Application.Localization;
using EventFlow.Application.Users.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Users.Validators
{
    public class UserLoginRequestModelValidator : AbstractValidator<UserLoginRequestModel>
    {
        public UserLoginRequestModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(x => ValidationMessages.EmailRequired)
                .EmailAddress().WithMessage(x => ValidationMessages.InvalidEmailFormat)
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage(x => ValidationMessages.InvalidEmailFormat);
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(x => ValidationMessages.PasswordRequired);
        }
    }
}
