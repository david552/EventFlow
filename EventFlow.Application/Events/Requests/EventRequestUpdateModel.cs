using EventFlow.Application.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventFlow.Application.Events.Requests
{
    public class EventRequestUpdateModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AvailableTickets { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime StartTime { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime EndTime { get; set; }
    }
}
