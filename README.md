# Buyly API

Buyly is a layered ASP.NET Core backend for modern e-commerce experiences. It follows a Clean Architecture-inspired structure (Domain → Application → Infrastructure → API) to isolate business rules from frameworks, powering catalog browsing, customer carts, secure checkout, PayPal-based payments, and admin workflows.

## Project Overview

Buyly exposes a RESTful API that covers the entire shopping funnel: product discovery, carting, order orchestration, payments, reviews, and administrator tooling. The solution follows a classic Domain/Application/Infrastructure/API split to keep business logic independent from transport and persistence, while leaning on .NET 8, Entity Framework Core, ASP.NET Identity, and PayPal’s two-step checkout flow. Rate limiting, custom middleware, and automated cleanup services keep the platform resilient in production.

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core + Pomelo MySQL provider
- ASP.NET Identity for user management and roles
- JWT bearer authentication with custom token service
- PayPal Checkout SDK (two-step capture flow) and Stripe.NET (planned)
- Hosted background services & rate limiting from `Microsoft.AspNetCore.RateLimiting`
- Custom middleware for error handling, validation, and security headers

### Runtime Stack Details

- Minimal hosting in `Program.cs` wires controllers, middleware, rate limiting, and Swagger before auth/authorization kicks in.
- `AppDbContext` + EF Core + Pomelo MySQL drive persistence, with Fluent configurations and migrations checked into `Buyly.Infrastructure/Data/Migrations`.
- ASP.NET Identity + `TokenService` issue JWTs with roles; `AuthService` handles login/register/reset flows.
- `PaymentService` integrates with PayPal’s two-step capture flow; `EmailSender` uses configurable SMTP credentials.
- `CartOrderCleanupService` runs in the background to cancel stale orders and clear abandoned carts using `CleanupSettings`.

## Architecture

- `Buyly.Domain`: Entities, enums, and specification abstractions that model products, orders, carts, payments, and reviews while remaining persistence-agnostic.
- `Buyly.Application`: DTOs, interfaces, service implementations, configuration objects, and domain-level exceptions. Use cases (auth, catalog, carts, orders, reviews) are orchestrated here.
- `Buyly.Infrastructure`: EF Core context, Fluent configuration classes, migrations, repositories, email sender, PayPal service, and background hosted services. This layer satisfies the interfaces defined in Application.
- `Buyly.API`: ASP.NET Core host with Program bootstrap, middleware pipeline, filters, controllers, and DI registration.

### Technical Highlights

- **Domain model**: Core entities (`Product`, `Category`, `Order`, `Payment`, `Review`, `ReviewVote`, `CartItem`) inherit from `BaseEntity` for consistent timestamps and IDs, while enums such as `OrderStatus` describe lifecycle transitions.
- **Specification-driven queries**: `ProductSpecification` and `ProductWithFiltersForCountSpecification` encapsulate filtering, sorting, pagination, and includes so controllers stay thin.
- **Unit of Work + Generic Repository**: `IUnitOfWork` coordinates transactional persistence, while `IGenericRepository<T>` and concrete repositories expose reusable data access patterns.
- **Identity + Roles**: `RoleSeeder` ensures `Customer` and `Admin` roles exist on startup. Auth flows run through `AuthService`, with custom `ITokenService` for JWT generation.
- **Rate Limiting**: `ServiceCollectionExtensions.AddRateLimitingPolicies` wires a global limiter and a stricter per-route partition keyed by IP, surfacing JSON error payloads with retry hints.
- **Security Middleware**: `SecurityHeadersMiddleware` and `ErrorHandlingMiddleware` enforce safe defaults and consistent API error responses.
- **Background Cleanup**: `CartOrderCleanupService` runs on a configurable cadence to cancel stale pending orders, restock inventory, and purge abandoned carts.

## Features

- **Authentication & Authorization**
  - JWT login/register with role claims and custom token lifetime control.
  - Forgot/reset password flow with hashed verification codes, configurable cooldown windows, and SMTP email delivery.
- **Catalog & Categories**
  - Public product listing with dynamic filters (search term, category, price range, sorting) powered by specifications.
  - Admin-only CRUD for products and categories through `[Authorize(Roles = Admin)]` controllers.
- **Carts & Orders**
  - Authenticated cart endpoints with strict rate limiting, per-item updates, and automatic stock restoration.
  - Order placement, status transitions (`PendingPayment`, `Processing`, `Cancelled`, etc.), and order item persistence.
- **Payments**
  - PayPal integration using `PaymentService` with intent creation, approval link storage, capture, and payer metadata.
  - Payment entities keep audit data (transaction IDs, currency, captured timestamps) for downstream reconciliation.
- **Reviews & Engagement**
  - Review CRUD, rating constraint enforcement via migrations, and vote tracking for helpfulness signals.
- **Ops & Reliability**
  - Hosted cleanup service resets abandoned carts/orders, freeing locked inventory automatically.
  - Global/strict rate limiting, validation filter, and middleware-based error handling to harden the API surface.
  - Swagger documentation with JWT auth definition for quick manual testing.

## API Usage

- Interactive documentation: launch Swagger UI at `/swagger` to explore endpoints, inspect payloads, and authorize with JWT tokens.
- Authentication: any non-public endpoint must send `Authorization: Bearer <jwt-token>` obtained from `/api/auth/login`. Tokens honor the signing/expiration rules in `JwtSettings`.
- Admin workflows: create an admin user manually (seed role + assign via `UserManager` or SQL) before trying the `/api/admin/*` routes.

## Folder Structure

```
Buyly-API/
├── Buyly.API/             # ASP.NET Core host: Program, controllers, middleware, DI
├── Buyly.Application/     # DTOs, interfaces, business services, configurations
├── Buyly.Infrastructure/  # EF Core context/migrations, repositories, integrations
├── Buyly.Domain/          # Entities, value objects, specifications, enums
├── Buyly.sln              # Solution file tying projects together
└── README.md
```

## How to Run the Project

1. **Prerequisites**: .NET 8 SDK, MySQL 8+, and `dotnet-ef` CLI (`dotnet tool install --global dotnet-ef` if needed).
2. **Clone**: `git clone https://github.com/your-org/Buyly-API.git` and `cd Buyly-API`.
3. **Configure secrets**: Update configuration (see “Configuration Setup” below) with MySQL, JWT, PayPal, rate limiting, email, and cleanup values. Keep real secrets out of source control.
4. **Apply migrations**:
   ```
   dotnet ef database update \
     --project Buyly.Infrastructure/Buyly.Infrastructure.csproj \
     --startup-project Buyly.API/Buyly.API.csproj
   ```
5. **Run the API**: `dotnet run --project Buyly.API/Buyly.API.csproj` (or `dotnet watch run` for hot reload). Swagger UI appears at `https://localhost:5001/swagger`.
6. **Seed roles**: Role seeding happens automatically at startup via `RoleSeeder`. Create admin accounts manually using the admin endpoints or Identity tooling.

### Database Preparation

Before running migrations, make sure MySQL is ready:

```sql
CREATE DATABASE BuylyDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'buyly'@'localhost' IDENTIFIED BY 'StrongPassword123!';
GRANT ALL PRIVILEGES ON BuylyDb.* TO 'buyly'@'localhost';
FLUSH PRIVILEGES;
```

- Replace the username/password with your own secure values and update `ConnectionStrings:DefaultConnection` accordingly.
- If MySQL runs on a non-default port or remote host, update the connection string (`Server`, `Port`, `Database`, etc.) to match.
- Run `dotnet ef database update` after switching credentials to ensure the schema (Identity + commerce tables, indexes, constraints) is applied.

## Configuration Setup

The app reads settings from `appsettings.json`, then overrides with `appsettings.{Environment}.json`, and finally environment variables/user secrets. Recommended workflow:

1. Keep `Buyly.API/appsettings.json` in source control with placeholders.
2. Create `Buyly.API/appsettings.Development.json` (ignored via `.gitignore`) for your local secrets, or use `dotnet user-secrets`.
3. For production/staging, rely on environment variables or Key Vault.

### Required Keys

| Section                 | Keys                                                                                | Description                                                                                                  |
| ----------------------- | ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| `ConnectionStrings`     | `DefaultConnection`                                                                 | MySQL connection string (`Server=localhost;Port=3306;Database=BuylyDb;User=buyly;Password=StrongPassword;`). |
| `JwtSettings`           | `SecretKey`, `Issuer`, `Audience`, `ExpirationMinutes`                              | Used by `TokenService` and JWT bearer auth. Secret must be at least 32 chars.                                |
| `PayPal`                | `ClientId`, `ClientSecret`, `Environment`                                           | Credentials for PayPal Checkout SDK (Sandbox or Live).                                                       |
| `RateLimiting`          | `GlobalLimit`, `StrictLimit` objects (`PermitLimit`, `WindowSeconds`, `QueueLimit`) | Controls global limiter and strict per-route limiter.                                                        |
| `EmailSettings`         | `SmtpHost`, `SmtpPort`, `SenderEmail`, `SenderName`, `Username`, `Password`         | Used by `EmailSender` for password reset notifications.                                                      |
| `ResetPasswordSettings` | `CooldownSeconds`, `WindowMinutes`, `WindowLimit`                                   | Governs reset code throttling logic in `AuthService`.                                                        |
| `CleanupSettings`       | `ExecutionIntervalMinutes`, `PendingOrderMaxAgeHours`, `RestoredCartMaxAgeHours`    | Controls `CartOrderCleanupService` cadence and thresholds.                                                   |

### Example `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=BuylyDb;User=buyly;Password=StrongPassword123;"
  },
  "JwtSettings": {
    "SecretKey": "replace-with-64-char-secret",
    "Issuer": "BuylyAPI",
    "Audience": "BuylyAPIUsers",
    "ExpirationMinutes": 1440
  },
  "PayPal": {
    "ClientId": "your-sandbox-client-id",
    "ClientSecret": "your-sandbox-secret",
    "Environment": "Sandbox"
  },
  "RateLimiting": {
    "GlobalLimit": { "PermitLimit": 200, "WindowSeconds": 60, "QueueLimit": 0 },
    "StrictLimit": { "PermitLimit": 20, "WindowSeconds": 60, "QueueLimit": 0 }
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "support@example.com",
    "SenderName": "Buyly Support",
    "Username": "support@example.com",
    "Password": "app-password"
  },
  "ResetPasswordSettings": {
    "CooldownSeconds": 30,
    "WindowMinutes": 15,
    "WindowLimit": 3
  },
  "CleanupSettings": {
    "ExecutionIntervalMinutes": 30,
    "PendingOrderMaxAgeHours": 12,
    "RestoredCartMaxAgeHours": 24
  }
}
```

> Tip: hide secrets with `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."` so team members can sync code without leaking credentials.
