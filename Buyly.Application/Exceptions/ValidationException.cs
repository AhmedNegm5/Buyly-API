using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.Application.Exceptions
{
    public class ValidationException : BaseException
    {
        public ValidationException(string[]? errors)
            : base("One or more validation errors occurred.", 400, errors)
        {
        }

        public ValidationException(string error)
            : base("One or more validation errors occurred.", 400, new[] { error })
        {
        }
    }
}