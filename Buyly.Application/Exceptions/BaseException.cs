using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; }
        public string[]? Errors { get; }

        public BaseException(string message, int statusCode, string[]? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}