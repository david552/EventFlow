using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public string Code { get; }

        public NotFoundException(string message, string code) : base(message)
        {
            Code = code;

        }
    }
}
