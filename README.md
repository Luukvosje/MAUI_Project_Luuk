# TimeOn — Kilometerregistratie

TimeOn is een .NET MAUI-app voor consultants, monteurs en accountmanagers die veel onderweg zijn. De app registreert automatisch gereden kilometers en klantbezoeken via GPS, en synchroniseert ritten en klantgegevens met een eigen ASP.NET Core Web API.

**Platformen:** Android en Windows  
**Stack:** .NET 10, C# 14, MAUI, JWT-authenticatie, SQL Server

---

## Vereisten

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [.NET MAUI-workload](https://learn.microsoft.com/dotnet/maui/get-started/installation): `dotnet workload install maui`
- **SQL Server** (LocalDB volstaat voor development; standaard in `appsettings.json`)
- Voor **Android**: Android SDK + emulator of fysiek apparaat
- Voor **Windows**: Windows 10/11 met Windows App SDK

---

## Projectstructuur

| Project | Rol |
|---------|-----|
| `TimeOn.Maui` | MAUI-app (UI, GPS-tracking, API-client) |
| `TimeOn.Api` | REST API met Swagger |
| `TimeOn.Application` | Business logic en services |
| `TimeOn.Domain` | Domeinmodellen |
| `TimeOn.Infrastructure` | Database, repositories, JWT |
| `TimeOn.UnitTests` | Unit tests |

Open de solution via `TimeOn.slnx` in Visual Studio of Rider.

---

## Installatie

### 1. Repository klonen en packages herstellen

```bash
git clone <repository-url>
cd MAUI_Project_Luuk
dotnet restore
```

### 2. API configureren

Pas indien nodig de database-connection string aan in `src/TimeOn.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TimeOn;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Pas ook de JWT- en Google API-instellingen aan in hetzelfde bestand voor een productieomgeving.

### 3. Database aanmaken

Voer Entity Framework-migraties uit vanuit de solution root:

```bash
dotnet ef database update --project src/TimeOn.Infrastructure --startup-project src/TimeOn.Api
```

### 4. Web API starten

```bash
dotnet run --project src/TimeOn.Api
```

De API draait op:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

Swagger UI: [https://localhost:5001/swagger](https://localhost:5001/swagger)

### 5. MAUI-app configureren

De API-URL staat in `src/TimeOn.Maui/appsettings.json`:

| Platform | Bestand | Standaard URL |
|----------|---------|---------------|
| Windows | `appsettings.json` | `https://localhost:5001/` |
| Android-emulator | `appsettings.android.json` | `http://10.0.2.2:5000/` |
| Android fysiek apparaat | `appsettings.android.json` | IP-adres van je PC, bijv. `http://192.168.x.x:5000/` |

Zorg dat de API bereikbaar is vanaf het apparaat waarop je de app draait.

### 6. MAUI-app starten

**Windows:**

```bash
dotnet build src/TimeOn.Maui -f net10.0-windows10.0.19041.0
dotnet run --project src/TimeOn.Maui -f net10.0-windows10.0.19041.0
```

**Android (emulator of apparaat aangesloten):**

```bash
dotnet build src/TimeOn.Maui -f net10.0-android
dotnet run --project src/TimeOn.Maui -f net10.0-android
```

Of start `TimeOn.Maui` als startup project in Visual Studio en kies het gewenste platform.

---

## Gebruik

### Account aanmaken en inloggen

1. Start de app — het login-scherm verschijnt automatisch.
2. Maak een account aan via **Register**, of log in met bestaande gegevens.
3. Na succesvolle login opent het hoofdscherm met tabbladen.

### Werkdag tracken

1. Ga naar het tabblad **Tracking**.
2. Druk op **Start tracking** om een werksessie te beginnen.
3. Geef locatie- en notificatierechten wanneer de app daarom vraagt (Android).
4. De app registreert GPS-punten en detecteert rijden en stilstand automatisch.
5. Druk op **Stop tracking** om de sessie te beëindigen en de rit naar de API te sturen.

> Op Android blijft tracking actief via een foreground service, ook wanneer de app op de achtergrond staat.

### Overige functies

| Tabblad | Functie |
|---------|---------|
| **Dashboard** | Overzicht van kilometers en statistieken |
| **Trips** | Lijst van geregistreerde ritten; tik op een rit voor details |
| **Customers** | Klanten beheren en op de kaart bekijken |
| **Settings** | GPS-notificaties, development mode (Windows), uitloggen |

### Development mode (Windows)

Schakel **Development mode** in via **Settings**. Op het tabblad **Tracking** kun je dan GPS-data plakken (JSON) om een werksessie handmatig te importeren — handig voor testen zonder echte GPS.

---

## Tests uitvoeren

```bash
dotnet test tests/TimeOn.UnitTests
```

---

## Veelvoorkomende problemen

| Probleem | Oplossing |
|----------|-----------|
| App kan API niet bereiken (Android) | Controleer of de API draait en of `appsettings.android.json` het juiste IP/URL gebruikt |
| Databasefouten bij opstarten API | Voer `dotnet ef database update` opnieuw uit |
| Locatie werkt niet | Controleer locatierechten in apparaatinstellingen |
| HTTPS-certificaat op Windows | Vertrouw het development-certificaat: `dotnet dev-certs https --trust` |
