using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.GlobalSettings.Requests
{
    public class GlobalSettingsRequestCreateModel
    {
        public string Key { get; set; } = null!;
        public int Value { get; set; }
    }
}
