using Buyly.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddConfiguredOptions(builder.Configuration)
    .AddControllersWithValidation()
    .AddRateLimitingPolicies(builder.Configuration)
    .AddSwaggerDocumentation()
    .AddDatabaseAndIdentity(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration, builder.Environment)
    .AddApplicationDependencies();

var app = builder.Build();

await app.SeedRolesAsync();
app.UseApplicationPipeline();
app.MapControllers();

app.Run();
