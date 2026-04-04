using EventFlow.Application.Events.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Events.Validators
{
    public class EventRequestCreateModelValidator : AbstractValidator<EventRequestCreateModel>
    {
        public EventRequestCreateModelValidator()
        {
            RuleFor(x => x.Title)
                 .NotEmpty().WithMessage("Event title cannot be empty.")
                 .MaximumLength(100).WithMessage("Title cannot contain more than 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Event description cannot be empty.") 
                .MaximumLength(2000).WithMessage("Description cannot contain more than 2000 characters.");

            RuleFor(x => x.TotalTickets)
                .GreaterThan(0).WithMessage("Event total tickets count must be greater than 0.");

            RuleFor(x => x.StartDate)
                .Must(date => date > DateTime.Now).WithMessage("Start date must be in the future.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");
        }


    }
    
}
