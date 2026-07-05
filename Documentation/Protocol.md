# Tour Planner — Project Protocol

## 1. Overview

Tour Planner is a two-tier application: an Angular 21 frontend and an ASP.NET Core 10 backend with a
PostgreSQL database accessed through Entity Framework Core. Users manage tours and the logs of
accomplished tours; the application computes derived attributes, supports full-text search, image upload,
statistics, elevation profiles and PDF reports.

## 2. Architecture

### 2.1 Layered backend

The backend enforces a layered architecture with a strictly downward dependency:

```
Controllers (Presentation)
      │  DTOs only
      ▼
Services (Business Layer)   ── validation, ownership, computed attributes, search, reports
      │  domain interfaces
      ▼
Repositories (Data Access)  ── EF Core queries
      │
      ▼
TourPlannerDbContext / PostgreSQL
```

- **Presentation** (`Controllers/`): thin controllers. They never reference `DbContext`; they read the
  authenticated user id from the JWT and delegate to the business layer, returning DTOs.
- **Business layer** (`Services/`): `TourService`, `TourLogService`, `UserService`, `StatisticsService`,
  `ReportService`, `RouteService`, plus `TourAttributeCalculator`. Owns all business rules.
- **Data access** (`Repositories/`): `TourRepository`, `TourLogRepository`, `UserRepository` behind
  interfaces — the Repository pattern over EF Core.
- **Cross-cutting**: DTOs (`Dtos/`) and manual mapping (`Mapping/`) keep entities off the API boundary;
  layer-owned exceptions (`Exceptions/`) are translated to RFC-7807 ProblemDetails by
  `Middleware/ExceptionHandlingMiddleware`, so no EF/HTTP-specific exception ever leaks upward.

### 2.2 Frontend (MVVM)

Angular standalone components with signal-based state services acting as view-models
(`TourStateService`, `UserStateService`). Components bind to signals; forms use reactive validation.
An HTTP interceptor attaches the JWT and logs the user out on 401; a route guard protects authenticated
routes. Leaflet renders the route map; custom SVG components render the statistics charts and the
elevation profile.

## 3. Design patterns 

- **Repository pattern** — `I*Repository` / `*Repository` isolate EF Core from the business layer.
- **Dependency Injection** — all services/repositories are registered in `Program.cs` and injected via
  constructors (frontend uses Angular DI / `inject()`).
- **Strategy** — `RouteService` maps the transport type to an OpenRouteService profile
  (Bike→cycling-regular, Hike→foot-hiking, Running→foot-running, Vacation→driving-car).
- **DTO + Mapper** — request/response DTOs decouple the wire format from the persistence model.

## 4. Key technical decisions, failures and solutions

- **Business logic moved server-side.** Computed attributes and search initially lived only in the Angular
  state service. They were moved into `TourService` / `TourAttributeCalculator` (thresholds bound from
  configuration) so they are authoritative and unit-testable; the client keeps a fallback for offline demo.
- **Authentication rewritten.** The original flow hashed passwords in the browser and the server trusted the
  hash; `GET /api/tours` returned every user's tours. This was replaced with **server-side BCrypt hashing**,
  **JWT** issuance, `[Authorize]`, and **per-user ownership** on every query (cross-user access → 403).
- **Images on the filesystem.** Images were previously a URL text field. They are now uploaded via a
  multipart endpoint, validated (type/size), stored on disk under a GUID name (base directory from config),
  and referenced by file name in the database — with a path-traversal guard on retrieval.
- **Elevation without a second API call.** Adding `elevation: true` to the existing ORS directions request
  returns 3D geometry, so ascent/descent and the profile are computed from data we already fetch.
- **Config leak fixed.** `appsettings.Development.json` (DB password + a live ORS key) was committed; it was
  de-tracked and `.gitignore`d, with an `.example` template documenting the shape.
- **PDF charts.** QuestPDF renders the elevation profile as inline SVG generated from the same geometry,
  avoiding a binary chart dependency.

## 5. Full-text search (sequence)

`GET /api/tours/search?q=` → `ToursController` reads the user id from the JWT → `TourService.SearchAsync`
loads the user's tours via `ITourRepository.GetAllByUserAsync`, computes popularity/child-friendliness with
`TourAttributeCalculator`, builds a per-tour "haystack" (name, description, from/to, transport, log text **and
the computed attributes**) and returns the tours where every query token matches. See the sequence diagram in
`UML.md`.

## 6. Unit tests — what is tested and why

45 NUnit tests (Moq for repositories) target the logic-bearing, high-risk code:

- **`PasswordHelper`** — security-critical: hashing must be salted and verification must reject wrong/invalid
  hashes.
- **`TourAttributeCalculator`** — the computed attributes drive search and statistics; tested at the tier
  boundaries where off-by-one errors hide.
- **`TourService` / `TourLogService`** — ownership enforcement (a security boundary), validation, that create
  stamps the owning user, and that search matches computed values.
- **`UserService`** — registration must never store a raw password and must reject duplicate emails; login
  must reject wrong/unknown credentials.
- **`RouteService.ComputeElevation`** — ascent/descent/min/max and profile from geometry; a pure function that
  is easy to get wrong on the sign of deltas.
- **`ImageStorageService`** — type/size/empty rejection and the path-traversal guard.

These are chosen because they encode business rules and security boundaries whose failure would be silent
and damaging, and because they are deterministic and fast (no database — repositories are mocked).

## 7. Time tracking

| Area | Hours |
|------|-------|
| Backend architecture (layers, DAL, BL, exceptions) |25|
| Authentication (JWT, hashing, ownership) |10|
| Computed attributes + search |9|
| Image upload |5|
| Statistics |9|
| Elevation profile |3|
| PDF reports |6|
| Frontend (components, MVVM, Leaflet) |19|
| Unit tests |4|
| Documentation |8|
| **Total** |98|

## 8. UI / UX

The main page is composed of visually separated components (header with global search + auth, navbar,
tour list, tour details, tour log list/forms). The design is responsive (CSS grid/flex + media queries) and
theme-aware. Statistics and the elevation profile use a colourblind-safe palette. The reusable
`app-image-upload` (drag-and-drop) and `app-map` / `app-elevation-profile` are the project's custom reusable
UI components. Wireframes are in `Wireframe early draft1.png` / `Wireframe early draft2.png`.

## 9. Extra Features

- **Statistics feature**: The backend exposes `GET /api/statistics` (implemented in `StatisticsController`) which returns a per-user `StatisticsDto` computed by the business layer. The statistics include totals (tours, logs), aggregated metrics (total logged distance, total logged time), averages (rating, difficulty), and a breakdown by transport type. The frontend renders these as compact KPI cards and small charts using the project's SVG chart components; the server-side `IStatisticsService` is the authoritative source used by both the API and the PDF summary generator. See [TourPlannerAPI/Controllers/StatisticsController.cs](TourPlannerAPI/Controllers/StatisticsController.cs).

- **PDF reports (generation & download)**: The API provides downloadable PDF reports in two places: a per-tour report (via `ReportService.GenerateTourReportAsync` exposed from the `ToursController`) and a fleet/summary report (`GET /api/statistics/report` in `StatisticsController`). PDFs are generated with QuestPDF and assembled from the business-layer DTOs. Reports embed the elevation profile and a small route sketch as inline SVG, include tour attributes and the logs table, and are served with the `application/pdf` content type for browser download. See [TourPlannerAPI/Services/ReportService.cs](TourPlannerAPI/Services/ReportService.cs).

- **Height / elevation profile**: The route geometry request to OpenRouteService is made with `elevation: true`, so the returned GeoJSON contains 3D coordinates. `RouteService` derives ascent/descent/min/max and a downsampled distance-vs-elevation profile from that 3D geometry (pure function `ComputeElevation`), which is used by the frontend's `app-elevation-profile` component and embedded in PDFs. This avoids extra elevation-only API calls and keeps the elevation logic unit-testable (`RouteServiceElevationTests`). See [TourPlannerAPI/Services/RouteService.cs](TourPlannerAPI/Services/RouteService.cs).

## 10. Git Link

> **Git repository:** https://github.com/Clemens-Heindl/Touring-Machine.git <owner>/<repo>`