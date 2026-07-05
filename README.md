# Tour Planner

A web application for planning bike / hike / running / vacation tours and tracking the logs and
statistics of accomplished tours. Users register and log in, create tours (with maps and routes from
OpenRouteService), record tour logs, and view computed statistics — including elevation profiles and
downloadable PDF reports.

## Tech stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 21 (standalone components, signals, MVVM) + Leaflet |
| Backend | ASP.NET Core 10 (C#) |
| O/R mapper | Entity Framework Core 10 (Npgsql) |
| Database | PostgreSQL (Docker) |
| Auth | JWT bearer + BCrypt password hashing |
| Logging | log4net (via `Microsoft.Extensions.Logging`) |
| Routing/Maps | OpenRouteService Directions API + Leaflet |
| PDF | QuestPDF |
| Tests | NUnit + Moq (45 tests) |

## Architecture

A layered architecture with a strict downward dependency (`Controller → Business Layer → Data Access → DbContext`):

- **Presentation** — `Controllers/` (thin, no `DbContext` access)
- **Business Layer** — `Services/` (validation, ownership, computed attributes, search, reports)
- **Data Access** — `Repositories/` (EF Core, Repository pattern)
- **DTOs / Mapping / Exceptions** — data cross the API boundary as DTOs; the layers throw their own
  domain exceptions, translated to RFC-7807 ProblemDetails by middleware.

See `Documentation/Protocol.md` and `Documentation/UML.md` for details.

## Features

- Self-registration and JWT login; every tour and log belongs to a single user (no sharing).
- Tour CRUD with name/description/from/to/transport/distance/estimated time and a Leaflet map; distance,
  time and route geometry come from OpenRouteService.
- Tour-log CRUD (date, comment, difficulty, distance, time, rating).
- Computed attributes: **popularity** (from log count) and **child-friendliness** (from difficulty/time/distance).
- Full-text search across tours, logs and the computed attributes.
- JSON import/export of tour data.
- **Drag-and-drop image upload** stored on the filesystem (only the file name is kept in the database).
- **Statistics dashboard** with KPI tiles and charts.
- **Elevation profile** derived from OpenRouteService 3D geometry.
- **PDF reports** (per-tour, with embedded elevation profile, and a fleet summary).

## Prerequisites

- .NET 10 SDK
- Node.js 20+ / npm
- Docker (for PostgreSQL)
- A free OpenRouteService API key (https://openrouteservice.org)

## Configuration (not committed)

Secrets are **not** in source control. Copy the template and fill in your own values:

```bash
cp TourPlannerAPI/appsettings.Development.json.example TourPlannerAPI/appsettings.Development.json
```

Then set in `TourPlannerAPI/appsettings.Development.json`:

- `ConnectionStrings:DefaultConnection` — your PostgreSQL connection string
- `OpenRouteService:ApiKey` — your ORS API key
- `Jwt:Key` — a long random secret (≥ 32 chars)

`ImageStorage:BaseDirectory` (default `ImageStore`) and `Logging:LogDirectory` (default `Logs`) are in
`appsettings.json` and can be overridden.

## Running

See `running.md`. In short, three terminals:

```bash
docker-compose up -d                         # 1) PostgreSQL
cd TourPlannerAPI && dotnet run --launch-profile http   # 2) API (auto-applies migrations)
cd frontend && npm install && npm start      # 3) Angular  ->  http://127.0.0.1:4200
```

## Tests

```bash
cd TourPlannerAPI.Tests
dotnet test
```
