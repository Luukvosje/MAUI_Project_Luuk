# TimeOn — Architecture & Coding Rules

This document defines the mandatory conventions for the **TimeOn** solution: a mileage and customer visit tracking system for mobile workers. It applies to all contributors and AI-assisted changes.

**Solution:** `TimeOn.sln`  
**Stack:** .NET 10, ASP.NET Core Web API, .NET MAUI, EF Core, Clean Architecture, DDD-lite, MVVM Toolkit.

---

## 1. Architecture Rules

### 1.1 Layer boundaries


| Layer          | Project                 | Responsibility                                         |
| -------------- | ----------------------- | ------------------------------------------------------ |
| Domain         | `TimeOn.Domain`         | Business rules, entities, value objects, domain events |
| Application    | `TimeOn.Application`    | Use cases, DTOs, validators, application services      |
| Infrastructure | `TimeOn.Infrastructure` | EF Core, repositories, JWT, external APIs              |
| API            | `TimeOn.Api`            | HTTP, controllers, middleware, API configuration       |
| Mobile         | `TimeOn.Mobile`         | UI, ViewModels, device services, offline/sync          |


### 1.2 Dependency direction (strict)

Dependencies must **only** point inward:

```text
TimeOn.Api        → Application, Infrastructure
TimeOn.Infrastructure → Application, Domain
TimeOn.Application    → Domain
TimeOn.Mobile         → Application (DTOs/contracts only; never Infrastructure)
TimeOn.Domain         → (nothing)
```

**Forbidden:**

- Domain referencing Application, Infrastructure, API, or MAUI
- Application referencing Infrastructure, API, or MAUI
- MAUI referencing Infrastructure or EF Core directly

### 1.3 DDD-lite principles

- Use **aggregates** (`User`, `Trip`, `Customer`) with clear consistency boundaries.
- Enforce rules **inside domain entities**, not only in services.
- Use **value objects** (`Email`, `GeoCoordinate`) for primitive obsession.
- Raise **domain events** for significant state changes (e.g. trip started/completed).
- Avoid anemic models: entities expose behavior, not only getters/setters.

### 1.4 Feature folder organization

- **Application:** `Features/{FeatureName}/` with `Commands/`, `Queries/`, `DTOs/`, `Validators/`, `Services/`
- **MAUI:** `Features/{FeatureName}/Views/`, `ViewModels/`, `Services/`
- **Tests:** mirror feature names under `Domain/`, `Application/`, `Api/`

### 1.5 Separation of concerns


| Concern                                | Where it lives                  |
| -------------------------------------- | ------------------------------- |
| HTTP / status codes                    | API controllers                 |
| Validation (input)                     | FluentValidation in Application |
| Business invariants                    | Domain entities                 |
| Data access                            | Infrastructure repositories     |
| UI state & commands                    | MAUI ViewModels                 |
| Cross-cutting (logging, auth pipeline) | API middleware & Infrastructure |


### 1.6 Intentionally excluded patterns

Do **not** introduce without explicit approval:

- MediatR / CQRS pipelines
- Microservices split
- Event sourcing
- Generic repository (`IRepository<T>`)
- Excessive abstraction layers or “framework inside the framework”

Use **simple application service classes** per feature instead of MediatR.

---

## 2. Domain Rules

### 2.1 Framework independence

`TimeOn.Domain` must not reference:

- Entity Framework Core
- ASP.NET Core
- MAUI / Xamarin
- Newtonsoft / System.Text.Json for persistence concerns

### 2.2 Rich entities

- Entities use **factory methods** (`Create`, `Schedule`) instead of public constructors where creation rules apply.
- Use **private setters** or private backing fields for mutable state.
- Collections exposed as `IReadOnlyCollection<T>`; mutate via domain methods.
- Throw `DomainException` for rule violations.

### 2.3 No anemic models

Avoid:

```csharp
// BAD: logic only in services
public void SetStatus(TripStatus status) => Status = status;
```

Prefer:

```csharp
// GOOD: trip controls its own lifecycle
public void Start(DateTime startedAt) { /* validate + transition */ }
```

### 2.4 Encapsulation

- No public setters on aggregate properties unless justified.
- Identity (`Guid Id`) assigned at creation; never changed.
- Value objects are immutable: prefer C# `record` types with `init` + a static `Create` factory (no separate equality base class).

### 2.5 Entity naming


| Type         | Convention         | Example                  |
| ------------ | ------------------ | ------------------------ |
| Entity       | Singular noun      | `Trip`, `Customer`       |
| Value object | Descriptive noun   | `Email`, `GeoCoordinate` |
| Domain event | Past tense + Event | `TripStartedEvent`       |
| Enum         | Singular           | `TripStatus`             |


### 2.6 Shared kernel

- `Entity`, `AggregateRoot` in `Domain/Shared/` (no shared `ValueObject` base — records cover structural equality)
- Domain-wide constants in `Domain/Constants/`
- Domain-specific exceptions in `Domain/Exceptions/`

---

## 3. API Rules

### 3.1 REST conventions

- Base route: `api/[controller]`
- Use HTTP verbs correctly: GET (read), POST (create/action), PUT/PATCH (update), DELETE (remove)
- Use plural resource names in URLs where applicable: `/api/trips`, `/api/customers`
- Return appropriate status codes: 200, 201, 400, 401, 404, 500

### 3.2 Controller naming

- Suffix: `Controller` → `TripsController`
- Thin controllers: delegate to Application services
- No business logic in controllers

### 3.3 DTO usage

- API accepts/returns **Application DTOs** or dedicated API models — never domain entities on the wire
- Do not leak EF entities or internal domain types

### 3.4 Validation

- Use **FluentValidation** in Application (`Features/*/Validators/`)
- Controllers do not manually validate every field; rely on validators + middleware
- Validation failures map to **400 Bad Request** with clear messages

### 3.5 Exception handling

- Use `ExceptionHandlingMiddleware` for global handling
- Map `DomainException` → 400
- Map `ValidationException` → 400
- Map `UnauthorizedAccessException` → 401
- Unhandled exceptions → 500 (logged, no sensitive details in response)

### 3.6 Authentication

- JWT Bearer authentication for protected endpoints
- Use `[Authorize]` on controllers/actions requiring auth
- Login via `AuthController`; token generation in Infrastructure (`JwtTokenService`)
- Secrets only in configuration (User Secrets / environment variables in production)

### 3.7 Swagger / OpenAPI

- Enabled in Development
- Document API title, version, and JWT security scheme
- Keep Swagger configuration in `Extensions/ServiceCollectionExtensions.cs`

### 3.8 Logging

- Use **Serilog** for structured logging
- Log requests via `UseSerilogRequestLogging()`
- Never log passwords or tokens

---

## 4. EF Core Rules

### 4.1 Fluent configurations only

- All mappings in `Infrastructure/Persistence/Configurations/`
- Implement `IEntityTypeConfiguration<T>`
- **No data annotations** on domain entities for persistence

### 4.2 DbContext

- Single `AppDbContext` in `Persistence/`
- Apply configurations via `ApplyConfigurationsFromAssembly`
- Migrations live in `Infrastructure/Migrations/` (generated when schema is ready)

### 4.3 Migration naming

When creating migrations:

```text
{Timestamp}_{DescriptiveName}
```

Examples: `20260521120000_InitialCreate`, `20260521130000_AddTripNotes`

### 4.4 Repository guidelines

- Define interfaces in **Application** (`Interfaces/Persistence/`)
- Implement in **Infrastructure** (`Repositories/`)
- Repositories are **feature-specific**, not generic `IRepository<T>`
- Methods express intent: `GetByUserIdAsync`, `GetAllActiveAsync`
- `SaveChanges` via `IUnitOfWork`, not per-repository saves scattered everywhere

### 4.5 SQL Server

- Provider: `Microsoft.EntityFrameworkCore.SqlServer`
- Connection string: `ConnectionStrings:DefaultConnection` or `Database:ConnectionString`
- Use transactions for multi-aggregate writes when needed

### 4.6 Owned types & encapsulation

- Map value objects with `OwnsOne` (e.g. `Email`, `GeoCoordinate`)
- Use backing fields for private collections when required (`_locationPoints`)

---

## 5. MAUI Rules

### 5.1 MVVM Toolkit (mandatory)

Use **CommunityToolkit.Mvvm**:


| Pattern                | Required                                                          |
| ---------------------- | ----------------------------------------------------------------- |
| Base class             | `ObservableObject`                                                |
| Properties             | `[ObservableProperty]` on **partial properties** (WinRT/AOT safe) |
| Commands               | `[RelayCommand]`                                                  |
| INotifyPropertyChanged | Generated by toolkit — do not implement manually                  |


Example:

```csharp
public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Email { get; set; }
}
```

### 5.2 Dependency injection (mandatory)

- Register services in `Extensions/ServiceCollectionExtensions.cs`
- **Constructor injection** in ViewModels, Pages, and services
- Register ViewModels and Pages as `Transient`; shell as `Singleton`
- Resolve `App` and `AppShell` via DI

### 5.3 No business logic in XAML

- XAML: layout, bindings, visual states only
- No calculations, API calls, or validation in XAML

### 5.4 Minimal code-behind

- Code-behind: `InitializeComponent()` + `BindingContext = viewModel`
- No command handlers in code-behind unless platform-specific

### 5.5 Shell navigation only

- Use `AppShell` with `TabBar` and registered routes
- Register routes in `Extensions/RoutingExtensions.cs`
- Navigate with `Shell.Current.GoToAsync()` — avoid `NavigationPage` stacks for main flow

### 5.6 Async/await

- All I/O-bound ViewModel methods are `async Task`
- Use `IsBusy` / `IsLoading` for UI feedback
- Catch exceptions at boundary; show user-friendly messages (when UI layer is implemented)

### 5.7 View / ViewModel naming


| Artifact  | Pattern              | Example          |
| --------- | -------------------- | ---------------- |
| Page      | `{Feature}Page`      | `TripsPage`      |
| ViewModel | `{Feature}ViewModel` | `TripsViewModel` |
| XAML      | Match class name     | `TripsPage.xaml` |


### 5.8 Mobile services

Interfaces in `Interfaces/`:

- `ILocationTrackingService`
- `INotificationService`
- `IApiService`
- `IAuthenticationService`
- `ILocalStorageService`
- `ISyncService`

Implementations in `Services/` or `Features/*/Services/`.

### 5.9 HttpClient

- Register typed clients: `AddHttpClient<IApiService, ApiService>()`
- Base URL from `Configuration/ApiSettings.cs`
- Never `new HttpClient()` in ViewModels

### 5.10 Offline-first structure


| Folder     | Purpose                                   |
| ---------- | ----------------------------------------- |
| `Offline/` | SQLite context, local entities            |
| `Sync/`    | Sync queue, background sync orchestration |
| `Caching/` | In-memory / disk cache abstractions       |


Rules:

- Writes go to local store first when offline
- Queue changes in `ISyncQueue` for later upload
- `ISyncService` coordinates push to API

### 5.11 Namespace conflict

`TimeOn.Mobile` references `TimeOn.Application`. In code-behind, use `Microsoft.Maui.Controls.Application` for the MAUI app class — not unqualified `Application`.

---

## 6. Application Layer Rules

### 6.1 Services over MediatR

- One service interface per feature area: `ITripService`, `IAuthService`
- Place in `Features/{Feature}/Services/`
- Methods are explicit: `GetByIdAsync`, `LoginAsync`

### 6.2 Commands and Queries folders

Reserved for future request records:

- `Commands/` — write operations (e.g. `StartTripCommand`)
- `Queries/` — read operations (e.g. `GetTripsByUserQuery`)

Handlers are **service methods**, not MediatR handlers.

### 6.3 Result pattern

Use `Result` / `Result<T>` in `Application/Common/` for expected failures without exceptions.

### 6.4 Validators

- One validator per request DTO
- Validators in `Features/*/Validators/`
- Register via `AddValidatorsFromAssembly`

---

## 7. Coding Rules

### 7.1 Language

- **English only** for code, comments, commits, and documentation
- User-facing strings may be localized later via resources

### 7.2 File naming

- Match primary type name: `TripService.cs` → `TripService`
- Interfaces: `ITripRepository.cs`
- One public type per file (exceptions for small related types)

### 7.3 Folder naming

- PascalCase folders: `Features`, `ViewModels`
- No spaces or abbreviations: use `Authentication`, not `Auth` in folder names only where already established (`Features/Auth` is acceptable for brevity)

### 7.4 Nullable reference types

- `<Nullable>enable</Nullable>` on all projects
- Avoid `null!` unless required for EF materialization
- Prefer explicit null checks or guard clauses

### 7.5 Treat warnings as errors

- Enforced via root `Directory.Build.props`: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- Fix all build warnings before merging

### 7.6 SOLID (practical)

- **S**ingle responsibility: small services, focused repositories
- **O**pen/closed: extend via new features, not modifying domain for every tweak
- **L**iskov: interfaces with clear contracts
- **I**nterface segregation: feature-specific interfaces, not god interfaces
- **D**ependency inversion: depend on abstractions in Application

### 7.7 Clean code

- Methods < 30 lines where possible
- Meaningful names over comments
- No commented-out dead code in commits
- No `#region` abuse

---

## 8. Dependency Injection Rules

### 8.1 Constructor injection only

```csharp
public class TripService(ITripRepository tripRepository, IUnitOfWork unitOfWork)
```

### 8.2 No service locator

Forbidden:

```csharp
var service = App.Services.GetService<ITripService>();
```

### 8.3 Lifetimes


| Registration                                  | Lifetime  | Examples       |
| --------------------------------------------- | --------- | -------------- |
| DbContext, Repositories, Application services | Scoped    | API request    |
| MAUI ViewModels, Pages                        | Transient | Per navigation |
| AppShell, settings                            | Singleton | App lifetime   |


### 8.4 Centralized registration


| Project        | Registration class                                                        |
| -------------- | ------------------------------------------------------------------------- |
| Application    | `Application/DependencyInjection/ApplicationServiceRegistration.cs`       |
| Infrastructure | `Infrastructure/DependencyInjection/InfrastructureServiceRegistration.cs` |
| API            | `Api/Extensions/ServiceCollectionExtensions.cs`                           |
| MAUI           | `Mobile/Extensions/ServiceCollectionExtensions.cs`                        |


### 8.5 Typed HttpClients

```csharp
services.AddHttpClient<IApiService, ApiService>(client => { ... });
```

---

## 9. Testing Rules

### 9.1 Frameworks

- **xUnit** for test runner
- **FluentAssertions** for readable assertions
- **NSubstitute** for mocks

### 9.2 Unit test naming

```text
{Method}_{Scenario}_{ExpectedResult}
```

Example: `Deactivate_WhenAlreadyInactive_ThrowsDomainException`

### 9.3 Arrange / Act / Assert

Structure every test:

1. **Arrange** — setup data and mocks
2. **Act** — invoke single method under test
3. **Assert** — one logical assertion per behavior

### 9.4 Mock external dependencies

- Unit tests: mock repositories, HTTP, GPS, storage
- Never hit real SQL or network in unit tests

### 9.5 Integration tests

- Project: `TimeOn.IntegrationTests`
- Use `WebApplicationFactory<Program>` for API tests
- Test HTTP status codes and contract shape
- Prefer in-memory DB or test containers when DB tests are added

### 9.6 Test folder layout

```text
tests/TimeOn.UnitTests/
  Domain/
  Application/{Feature}/

tests/TimeOn.IntegrationTests/
  Api/{Feature}/
  Infrastructure/
```

---

## 10. Security & Configuration

- Never commit secrets; use User Secrets / Azure Key Vault / environment variables
- Rotate `Jwt:SecretKey` in production
- HTTPS required in production
- Mobile stores tokens via `ILocalStorageService` — never plain-text passwords

---

## 11. Git & Workflow

- Small, focused commits
- Branch naming: `feature/`, `fix/`, `chore/`
- Pull requests must build locally: `dotnet build TimeOn.sln`
- Run tests: `dotnet test TimeOn.sln`

---

## 12. Implementation Phases (current status)


| Phase                                            | Status          |
| ------------------------------------------------ | --------------- |
| Solution structure & DI                          | Done            |
| Domain model scaffold                            | Done            |
| API pipeline (Swagger, JWT, Serilog, exceptions) | Done            |
| MAUI MVVM shell & services                       | Done            |
| Business logic (trips, visits, mileage)          | **Not started** |
| Full UI                                          | **Not started** |
| EF migrations                                    | **Not started** |


When implementing features, follow this document first. Propose changes to these rules via PR discussion before adopting new patterns.

---

*Last updated: solution scaffold generation.*