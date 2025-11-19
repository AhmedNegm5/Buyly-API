using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message)
            : base(message, 404)
        {
        }

        public NotFoundException(string resourceName, string key)
            : base($"Resource '{resourceName}' with key '{key}' was not found.", 404)
        {
        }

    }
}