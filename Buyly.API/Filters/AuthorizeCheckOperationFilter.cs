using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Buyly.API.Filters
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionAttributes = context.MethodInfo.GetCustomAttributes(true) ?? Array.Empty<object>();
            var controllerAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? Array.Empty<object>();

            var allowsAnonymous = actionAttributes.OfType<AllowAnonymousAttribute>().Any() ||
                                  controllerAttributes.OfType<AllowAnonymousAttribute>().Any();

            if (allowsAnonymous)
            {
                return;
            }

            var requiresAuth = actionAttributes.OfType<AuthorizeAttribute>().Any() ||
                               controllerAttributes.OfType<AuthorizeAttribute>().Any();

            if (!requiresAuth)
            {
                return;
            }

            operation.Security ??= new List<OpenApiSecurityRequirement>();

            var scheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [scheme] = new List<string>()
            });
        }
    }
}


