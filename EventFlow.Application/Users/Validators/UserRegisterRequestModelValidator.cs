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
    public class UserRegisterRequestModelValidator : AbstractValidator<UserRegisterRequestModel>
    {
        public UserRegisterRequestModelValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(x => ValidationMessages.FirstNameRequired)
                .MaximumLength(50).WithMessage(x => ValidationMessages.FirstNameMaxLength);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(x => ValidationMessages.LastNameRequired)
                .MaximumLength(50).WithMessage(x => ValidationMessages.LastNameMaxLength);

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(x => ValidationMessages.UsernameRequired)
                .MinimumLength(3).WithMessage(x => ValidationMessages.UsernameMinLength)
                .MaximumLength(50).WithMessage(x => ValidationMessages.UsernameMaxLength);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(x => ValidationMessages.EmailRequired)
                .EmailAddress().WithMessage(x => ValidationMessages.InvalidEmailFormat)
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage(x => ValidationMessages.InvalidEmailFormat);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(x => ValidationMessages.PasswordRequired)
                .MinimumLength(6).WithMessage(x => ValidationMessages.PasswordMinLength);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage(x => ValidationMessages.ConfirmPasswordRequired)
                .Equal(x => x.Password).WithMessage(x => ValidationMessages.PasswordsDoNotMatch); 
        }
    }
}
