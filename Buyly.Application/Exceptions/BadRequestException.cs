using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException(string message, string[]? errors = null)
            : base(message, 400, errors)
        {
        }
    }
}