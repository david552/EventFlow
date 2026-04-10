using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.GlobalSettings.Responses
{
    public class GlobalSettingsResponseModel
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public int Value { get; set; }
    }
}
