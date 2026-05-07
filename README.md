# TimeOn Mobile - Kilometerregistratie

Schoolproject in `.NET MAUI` voor automatische kilometerregistratie en klantbezoek-detectie op Android en iOS.

## Tech stack

- .NET 10
- C# 14
- .NET MAUI
- MVVM (CommunityToolkit.Mvvm)
- DI + Services + Repositories

## Project structuur

- `src/TimeOn.Mobile.App` - MAUI UI, Views, ViewModels, DI setup
- `src/TimeOn.Mobile.Core` - domeinmodellen, interfaces, use-cases
- `src/TimeOn.Mobile.Infrastructure` - device/API/storage/repository implementaties
- `tests/TimeOn.Mobile.UnitTests` - unit tests voor business logic

## Installatie

1. Installeer .NET SDK 10.
2. Controleer MAUI workloads:
   - `dotnet workload list`
3. Restore dependencies:
   - `dotnet restore TimeOn.Mobile.slnx`

## Build en run

- Build hele solution:
  - `dotnet build TimeOn.Mobile.slnx`
- Run op Android emulator/device:
  - `dotnet build src/TimeOn.Mobile.App/TimeOn.Mobile.App.csproj -f net10.0-android`
- Run op iOS simulator (op macOS):
  - `dotnet build src/TimeOn.Mobile.App/TimeOn.Mobile.App.csproj -f net10.0-ios`

## API gebruik

- API configuratie staat in `src/TimeOn.Mobile.App/appsettings.json`.
- Auth en API plumbing zitten in `src/TimeOn.Mobile.Infrastructure/Api`.
- Huidige startup gebruikt een mock login token; deze is bedoeld als vervangbaar startpunt voor echte JWT-flow.

## Testen

- Unit tests uitvoeren:
  - `dotnet test tests/TimeOn.Mobile.UnitTests/TimeOn.Mobile.UnitTests.csproj`

## Architectuurkeuzes

- MVVM voor strikte scheiding UI en business logic.
- Repository pattern voor ritten/bezoeken data access.
- Service abstractions voor GPS en notificaties.
- Device functionaliteit geabstraheerd achter interfaces voor testbaarheid.

## Bekende beperkingen en vervolgstappen

- Background tracking per platform is nog basic en moet productieklaar gemaakt worden.
- API-authenticatie is nu mock/stub en moet gekoppeld worden aan echte Web API endpoints.
- UI/device integration tests moeten nog als aparte stap toegevoegd worden.
