# Repository Guidelines

## Project Structure & Module Organization

This is an ASP.NET Core MVC school management application targeting .NET 10. `Controllers/` handles requests, `Models/` and `Models/ViewModels/` hold domain and screen data, and `Views/` contains Razor pages organized by feature. `data/AppDbContext.cs` configures Entity Framework Core; `Migrations/` contains database schema changes. Static assets are in `wwwroot/`, startup and service configuration are in `Program.cs`, and `SchoolManagementDB.sql` is a database script. Keep feature code in the existing MVC structure and avoid committing generated `bin/` or `obj/` output.

## Build, Test, and Development Commands

Run commands from the repository root, beside `School Management System.csproj`:

- `dotnet restore` downloads project dependencies.
- `dotnet build` compiles the application.
- `dotnet run` starts the web application; `dotnet watch run` enables hot reload.
- `dotnet ef database update` applies pending migrations to the configured SQL Server database.
- After changing an EF model, create a migration with `dotnet ef migrations add DescriptiveChangeName`, then apply it with `dotnet ef database update`.

The connection string and local launch URLs are documented in `guide.txt`. Configure a reachable SQL Server before running database commands.

## Coding Style & Naming Conventions

Follow existing C# and Razor conventions: four-space indentation, nullable reference types, PascalCase for types and public members, and camelCase for locals and parameters. Use descriptive MVC names such as `AttendanceController` and keep view models in `Models/ViewModels/`. Keep business logic out of Razor views and preserve existing route and feature naming patterns. No formatter or linter is configured in the project file.

## Testing Guidelines

No test project is currently present. Run `dotnet build` after changes and verify affected screens and workflows locally, including database-backed behavior when applicable. If adding tests, use a dedicated test project and descriptive names such as `AttendanceControllerTests`.

## Commit & Pull Request Guidelines

Recent commits use short, direct subjects, often lowercase (for example, `ignore uploads during hot reload`). Keep commit subjects concise and focused. Pull requests should describe the behavior changed, link related issues when available, note migration or configuration impacts, and include screenshots for UI changes. State which build and manual verification steps were completed.

## Security & Configuration

Do not commit real credentials or machine-specific connection strings. Use local configuration for secrets and keep safe placeholders in shared examples. Treat database reset operations as destructive; preserve user data when developing migrations.
