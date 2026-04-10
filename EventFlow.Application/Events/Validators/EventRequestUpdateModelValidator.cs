using EventFlow.Application.Events.Requests;
using EventFlow.Application.Localization;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Events.Validators
{
    public class EventRequestUpdateModelValidator : AbstractValidator<EventRequestUpdateModel>
    {
        public EventRequestUpdateModelValidator()
        {
            RuleFor(x => x.Title)
                  .NotEmpty().WithMessage(x => ValidationMessages.TitleRequired)
                  .MaximumLength(100).WithMessage(x => ValidationMessages.TitleMaxLength);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(x => ValidationMessages.DescriptionRequired)
                .MaximumLength(2000).WithMessage(x => ValidationMessages.DescriptionMaxLength);

            RuleFor(x => x.AvailableTickets)
                .GreaterThan(0).WithMessage(x => ValidationMessages.TicketsGreaterThanZero);

            RuleFor(x => x.StartTime)
                .Must(date => date > DateTime.Now).WithMessage(x => ValidationMessages.StartDateInFuture);

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage(x => ValidationMessages.EndDateAfterStartDate);
        }
    }
}
