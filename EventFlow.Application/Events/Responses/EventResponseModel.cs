using EventFlow.Application.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventFlow.Application.Events.Responses
{
    public class EventResponseModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int AvailableTickets { get; set; }
        public int UserId { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime StartTime { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime CreatedAt { get; set; }
    }
}
