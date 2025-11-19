using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Buyly.API.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                var response = ApiResponse.Error("One or more validation errors occurred.", errors);
                context.Result = new BadRequestObjectResult(response);
            }
        }
    }
}