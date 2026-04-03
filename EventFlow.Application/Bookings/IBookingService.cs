using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Bookings.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Bookings
{
    public interface IBookingService
    {
        Task<int> CreateAsync(BookingRequestModel model, int currentUserId, CancellationToken token);

        Task<List<BookingResponseModel>> GetUserBookingsAsync(int userId, CancellationToken token);

        Task BuyAsync(int bookingId, int currentUserId, CancellationToken token);

        Task CancelAsync(int bookingId, int currentUserId, CancellationToken token);
    }
}
