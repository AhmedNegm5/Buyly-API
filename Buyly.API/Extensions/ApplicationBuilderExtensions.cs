using Buyly.API.Middlewares;
using Buyly.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace Buyly.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedRolesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await RoleSeeder.SeedRolesAsync(roleManager);
        }

        public static WebApplication UseApplicationPipeline(this WebApplication app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<SecurityHeadersMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Buyly E-Commerce API v1");
                    c.DisplayRequestDuration();
                    c.EnableDeepLinking();
                    c.EnableFilter();
                    c.DefaultModelsExpandDepth(-1);
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}

