using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Localization;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Bookings.Validators
{
    public class BookingRequestCreateModelValidator : AbstractValidator<BookingRequestCreateModel>
    {
        public BookingRequestCreateModelValidator()
        {
            RuleFor(x => x.BookedTicketsCount)
                .GreaterThan(0).WithMessage(x => ValidationMessages.BookedTicketsGreaterThanZero);

            RuleFor(x=>x.EventId)
                .GreaterThan(0).WithMessage(x => ValidationMessages.EventIdGreaterThanZero);

        }

    }
}
