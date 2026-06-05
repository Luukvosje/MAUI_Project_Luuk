# TimeOn Architectuur — Klassendiagrammen

Klassendiagrammen voor de huidige solution-structuur (`TimeOn.Domain`, `TimeOn.Application`, `TimeOn.Infrastructure`, `TimeOn.Api`, `TimeOn.Mobile`).

**Laatst bijgewerkt:** juni 2026

---

## Solution-lagen

```mermaid
flowchart TB
    subgraph Presentation
        MAUI[TimeOn.Mobile]
        API[TimeOn.Api Controllers]
    end

    subgraph Application
        APP[Features / Services / DTOs]
    end

    subgraph Domain
        DOM[Entities / ValueObjects / Domain Services]
    end

    subgraph Infrastructure
        INF[EF Core / Repositories / JWT / Password hashing]
    end

    MAUI --> APP
    API --> APP
    APP --> DOM
    INF --> DOM
    API --> INF
    MAUI -.->|HTTP + JWT| API
```



---

## Domain

```mermaid
classDiagram
    direction TB

    class Entity {
        <<abstract>>
        +Guid Id
    }

    class Result {
        +bool IsSuccess
        +string Error
        +Success() Result
        +Failure(string) Result
    }

    class Result~T~ {
        +T Value
        +bool IsSuccess
        +string Error
    }

    class WorkSessionStatus {
        <<enumeration>>
        Active
        Stopped
        Cancelled
    }

    class SegmentType {
        <<enumeration>>
        Driving
        Stationary
    }

    class User {
        +Guid UserGuid
        +string Name
        +Email Email
        +HashedPassword Password
        +Register(...) User
        +Authenticate(...) Result
    }

    class Customer {
        +string Name
        +Guid UserId
        +string Address
        +string ContactEmail
        +bool IsActive
        +Coordinate Location
        +DateTime LastSyncedAtUtc
        +Create(...) Customer
        +UpdateDetails(...)
    }

    class WorkSession {
        +Guid UserId
        +DateTime StartTimeUtc
        +DateTime EndTimeUtc
        +double TotalDistanceKm
        +WorkSessionStatus Status
        +Start(...) WorkSession
        +ApplyClassifiedSegments(...) Result
        +Stop(...) Result
        +Cancel(...) Result
    }

    class GpsSegment {
        <<abstract>>
        +Guid WorkSessionId
        +SegmentType Type
        +DateTime StartUtc
        +DateTime EndUtc
        +IReadOnlyList~GpsPoint~ Points
        +TimeSpan Duration
        +Create(...) GpsSegment$
    }

    class DrivingSegment {
        +double DistanceMeters
        +double DistanceKm
    }

    class StationarySegment {
        +double CenterLatitude
        +double CenterLongitude
        +Guid CustomerId
        +double DistanceFromCustomerMeters
        +bool IsCustomerVisit
    }

    class GpsPoint {
        <<record>>
        +Coordinate Location
        +DateTime RecordedAtUtc
        +Create(...) GpsPoint$
    }

    class Coordinate {
        <<value object>>
        +double Latitude
        +double Longitude
        +DistanceTo(Coordinate) double
    }

    class Distance {
        <<value object>>
        +double Meters
        +Between(Coordinate, Coordinate)$ Distance
    }

    class Email {
        <<value object>>
        +string Value
    }

    class HashedPassword {
        <<value object>>
        +string Value
    }

    class SegmentClassifier {
        +Classify(IReadOnlyList~GpsPoint~) IReadOnlyList~GpsSegment~
    }

    Entity <|-- User
    Entity <|-- Customer
    Entity <|-- WorkSession
    Entity <|-- GpsSegment
    GpsSegment <|-- DrivingSegment
    GpsSegment <|-- StationarySegment

    WorkSession --> WorkSessionStatus
    WorkSession "1" --> "*" DrivingSegment : _drivingSegments
    WorkSession "1" --> "*" StationarySegment : _stationarySegments
    GpsSegment --> SegmentType
    GpsSegment o--> GpsPoint : Points not mapped
    User *-- Email
    User *-- HashedPassword
    Customer *-- Coordinate
    GpsPoint *-- Coordinate
    DrivingSegment ..> Distance : calculates via
    SegmentClassifier ..> GpsSegment : produces
```



---

## Domeininterfaces en infrastructuur

```mermaid
classDiagram
    direction LR

    class IWorkSessionRepository {
        <<interface>>
        +GetByIdWithDetailsAsync(...)
        +GetActiveByUserIdAsync(...)
        +AddAsync(WorkSession)
        +Update(WorkSession)
        +DeleteAsync(...)
    }

    class ICustomerRepository {
        <<interface>>
    }

    class IUserRepository {
        <<interface>>
    }

    class IPasswordHasher {
        <<interface>>
        +Hash(string) string
        +Verify(string, string) bool
    }

    class WorkSessionRepository {
        -AppDbContext _remoteContext
    }

    class CustomerRepository
    class UserRepository
    class PasswordHasher
    class JwtTokenService

    class TimeOnDbContextBase {
        <<abstract>>
        +DbSet WorkSessions
        +DbSet DrivingSegments
        +DbSet StationarySegments
        +DbSet Customers
        +DbSet Users
    }

    class AppDbContext

    IWorkSessionRepository <|.. WorkSessionRepository
    ICustomerRepository <|.. CustomerRepository
    IUserRepository <|.. UserRepository
    IPasswordHasher <|.. PasswordHasher

    WorkSessionRepository --> AppDbContext
    AppDbContext --|> TimeOnDbContextBase
    TimeOnDbContextBase --> WorkSession
    TimeOnDbContextBase --> DrivingSegment
    TimeOnDbContextBase --> StationarySegment
```



---

## Application (Worksession)

```mermaid
classDiagram
    direction TB

    class ICurrentUserAccessor {
        <<interface>>
        +Guid UserId
    }

    class IWorkSessionService {
        <<interface>>
        +CompleteFromTrackingAsync(...) Result~CompleteWorkSessionResponse~
        +GetWorkSessionDetailsAsync(...) Result~WorkSessionDetailDto~
        +DeleteAsync(...) Result
    }

    class IWorkSessionCompletionService {
        <<interface>>
        +Complete(WorkSession, List~GpsPoint~, DateTime) Result~WorkSession~
    }

    class WorkSessionService {
        -IWorkSessionRepository _workSessionRepository
        -ICurrentUserAccessor _currentUserAccessor
    }

    class WorkSessionCompletionService {
        -SegmentClassifier _classifier
        +Complete(...) Result~WorkSession~
    }

    class CompleteWorkSessionRequest {
        <<record>>
        +Guid SessionId
        +DateTime StartTimeUtc
        +DateTime EndTimeUtc
        +IReadOnlyList~GpsPointDto~ GpsPoints
    }

    class CompleteWorkSessionResponse {
        <<record>>
    }

    class WorkSessionDetailDto {
        <<record>>
    }

    class WorkSessionSegmentDto {
        <<record>>
    }

    class GpsPointDto {
        <<record>>
    }

    class WorkSessionsController {
        -IWorkSessionService _workSessionService
        -ICurrentUserAccessor _currentUserAccessor
        +Complete(...)
        +GetDetails(...)
        +Delete(...)
    }

    IWorkSessionService <|.. WorkSessionService
    IWorkSessionCompletionService <|.. WorkSessionCompletionService
    WorkSessionService --> IWorkSessionRepository
    WorkSessionService --> IWorkSessionCompletionService
    WorkSessionService --> ICurrentUserAccessor
    WorkSessionCompletionService --> SegmentClassifier
    WorkSessionCompletionService --> WorkSession
    WorkSessionsController --> IWorkSessionService
    WorkSessionService ..> CompleteWorkSessionRequest
    WorkSessionService ..> CompleteWorkSessionResponse
```



> **Opmerking:** `IWorkSessionCompletionService` / `WorkSessionCompletionService` zijn geregistreerd in DI en gedekt door unittests; koppel het constructorveld in `WorkSessionService` als het project nog niet compileert.

---

## Mobile — GPS-tracking

```mermaid
classDiagram
    direction TB

    class IGpsTrackingService {
        <<interface>>
        +TrackingState State
        +Guid CurrentSessionId
        +StartAsync()
        +StopAsync()
    }

    class GpsTrackingService {
        -ITrackingGpsStore _gpsStore
        -IApiService _apiService
        -IPlatformLocationTracker _platformTracker
        -ActiveTrackingSession _activeSession
    }

    class ITrackingGpsStore {
        <<interface>>
        +GetActiveSessionAsync(Guid)
        +SaveActiveSessionAsync(...)
        +AddPointAsync(Guid, GpsPoint)
        +GetPointsAsync(Guid)
    }

    class SqliteTrackingStore {
        -SQLiteAsyncConnection _database
    }

    class IPlatformLocationTracker {
        <<interface>>
        +StartAsync()
        +StopAsync()
    }

    class PollingLocationTracker
    class AndroidPlatformLocationTracker {
        <<Android>>
    }

    class GpsSampleEvaluator {
        <<static>>
        +ShouldSave(GpsPoint, LocationReading) bool
    }

    class TrackedGpsSample {
        +int Id
        +string WorkSessionId
        +double Latitude
        +double Longitude
        +DateTime RecordedAtUtc
    }

    class ActiveTrackingSession {
        <<record>>
        +Guid Id
        +Guid UserId
        +DateTime StartTimeUtc
    }

    class TrackingViewModel {
        -IGpsTrackingService _trackingService
    }

    IGpsTrackingService <|.. GpsTrackingService
    ITrackingGpsStore <|.. SqliteTrackingStore
    IPlatformLocationTracker <|.. PollingLocationTracker
    IPlatformLocationTracker <|.. AndroidPlatformLocationTracker

    GpsTrackingService --> ITrackingGpsStore
    GpsTrackingService --> IPlatformLocationTracker
    GpsTrackingService ..> CompleteWorkSessionRequest : on stop
    SqliteTrackingStore --> TrackedGpsSample
    SqliteTrackingStore --> ActiveTrackingSession
    GpsTrackingService --> GpsSampleEvaluator
    TrackingViewModel --> IGpsTrackingService
```



---

## Application

```mermaid
classDiagram
    direction LR

    class IAuthService {
        <<interface>>
    }
    class AuthService
    class ICustomerService {
        <<interface>>
    }
    class CustomerService
    class ITripService {
        <<interface>>
    }
    class TripService

    IAuthService <|.. AuthService
    ICustomerService <|.. CustomerService
    ITripService <|.. TripService

    AuthService --> IUserRepository
    AuthService --> IPasswordHasher
    AuthService --> IJwtTokenService
    CustomerService --> ICustomerRepository
    TripService --> IWorkSessionRepository
```



---

## Belangrijke ontwerpkeuzes


| Onderwerp      | Keuze                                                                   |
| -------------- | ----------------------------------------------------------------------- |
| Aggregate root | `WorkSession` bevat rij- en stilstaande segmenten                       |
| GPS-opslag     | Ruwe punten alleen op apparaat; API slaat geclassificeerde segmenten op |
| Klantbezoek    | `StationarySegment` met `CustomerId`                                    |
| Classificatie  | `SegmentClassifier` in Domein; afronding georkestreerd in Applicatie    |
| Authenticatie  | `User`-entiteit + JWT; `ICurrentUserAccessor` in API-pipeline           |


---

## Bronbestanden


| Gebied          | Pad                                                                   |
| --------------- | --------------------------------------------------------------------- |
| Entiteiten      | `src/TimeOn.Domain/Entities/`                                         |
| Value objects   | `src/TimeOn.Domain/Objects/` (namespace `TimeOn.Domain.ValueObjects`) |
| Classifier      | `src/TimeOn.Domain/Services/GpsClassifier.cs` (`SegmentClassifier`)   |
| Applicatie      | `src/TimeOn.Application/Features/`                                    |
| Persistentie    | `src/TimeOn.Infrastructure/Persistence/`                              |
| Mobile tracking | `src/TimeOn.Mobile/Features/Tracking/`                                |
| API             | `src/TimeOn.Api/Controllers/`                                         |


