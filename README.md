# AmosWRN (Sample Weather + Products Web App)

This is a small demo ASP.NET Core web application (Razor / MVC views) that showcases:

- Cookie-based authentication (simple file-backed user store)
- Product management with file-backed persistence (`products.json`)
- Image uploads for products and user avatars (stored under `wwwroot/uploads`)
- Simple UI with Bootstrap

This project is intended for learning and local demos only — it is not production hardened.

## Prerequisites

- .NET 10 SDK (install from https://dotnet.microsoft.com)
- Visual Studio 2022/2026 or `dotnet` CLI

## Run locally

Using the CLI:

1. Open a terminal at the project root (where `WebApplication1.csproj` is located):

   `dotnet build`

   `dotnet run --project WebApplication1`

2. Open the app in the browser (the URL is printed by `dotnet run`, typically `https://localhost:5001`).

Using Visual Studio:

- Open the solution and run (F5 / Debug) or Start Without Debugging (Ctrl+F5).

## Features / Endpoints

- Home page: `/` — shows featured products, product list and an inline "Add product" form (authenticated users only).
- Product create: `Home/Create` — accepts name, description, price and multiple image files.
- Product edit: `Home/Edit/{id}` — edit product and add/remove images (requires authentication).
- Account register: `Account/Register` — create a local user (stored in `users.json`).
- Account login: `Account/Login` — sign in with email + password.
- Profile: `Account/Profile` — change username and upload avatar.

API and utility endpoints:

- `/ping` — returns `pong` for quick checks.
- `/swagger` — lightweight docs page provided by the sample.

## Data storage

- Users are persisted to `users.json` in the app content root.
- Products are persisted to `products.json` in the app content root.
- Uploaded files are stored under `wwwroot/uploads`:
  - Product images: `wwwroot/uploads/products`
  - Avatars: `wwwroot/uploads/avatars`

## Important notes and limitations

- This is a demo app. Security and robustness are intentionally minimal:
  - Passwords are currently encoded with Base64 (insecure). Replace with `PasswordHasher<T>` or ASP.NET Core Identity before production.
  - No strict validation of uploaded image MIME types or file sizes. Add checks to prevent abuse.
  - File-backed JSON stores are not suitable for concurrent production use; use a database (EF Core + SQL/SQLite/Postgres) for production.
  - No email confirmation, rate-limiting, or account recovery flows are implemented.

## Suggested next steps (if you want to improve the app)

- Replace Base64 password storage with `PasswordHasher<T>` and/or integrate ASP.NET Core Identity.
- Add image validation (MIME types, size limits) and server-side resizing for thumbnails.
- Replace file-backed repositories with EF Core and a real database.
- Add client-side validation (unobtrusive validation) and stronger server-side validation using view models.
- Add product delete and role-based authorization for admin tasks.

## Contributing / Development notes

- Project entry point: `WebApplication1/Program.cs`.
- Controllers live under `WebApplication1/Controllers`.
- Views are in `WebApplication1/Views`.
- Data repositories are in `WebApplication1/Data`.
- Models are in `WebApplication1/Models`.

If you want me to apply any of the suggested improvements above (password hashing, image validation, EF Core migration, client-side validation), tell me which one and I will implement it.
