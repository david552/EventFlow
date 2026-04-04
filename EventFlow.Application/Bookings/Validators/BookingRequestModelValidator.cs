using EventFlow.Application.Bookings.Requests;
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
                .GreaterThan(0).WithMessage("Booked Tickets Count Must be greater than 0");

            RuleFor(x=>x.EventId)
                .GreaterThan(0).WithMessage("Event ID Must be greater than 0");

        }

    }
}
