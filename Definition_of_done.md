# Definition of Done – .NET MAUI Kilometerregistratie App

## 1. Functionele eisen

* De functionaliteit voldoet aan de beschreven user story en acceptatiecriteria.
* De functionaliteit werkt correct op minimaal twee platformen:

  * Android
  * iOS
* De gebruiker kan de functionaliteit zelfstandig uitvoeren zonder crashes of blokkerende fouten.
* Data wordt correct opgeslagen en opgehaald via de Web API.
* GPS-, notificatie- en bewegingsfunctionaliteit werken zoals bedoeld.
* Privacygevoelige data wordt alleen opgeslagen wanneer noodzakelijk.
* Ritten en klantbezoeken kunnen correct worden weergegeven en aangepast.

## 2. Codekwaliteit

* De applicatie is ontwikkeld in:

  * C# 14
  * .NET 10
  * .NET MAUI
* De codebase is volledig in het Engels geschreven.
* De code volgt consistente naamgeving en formatting conventions.
* Er zijn geen ongebruikte classes, methods of packages aanwezig.
* SOLID-principes zijn aantoonbaar toegepast waar relevant.
* Er is bewust gebruik gemaakt van OO-principes zoals:

  * encapsulation
  * inheritance/composition
  * abstraction
* Minimaal één relevant design pattern is toegepast en onderbouwd, bijvoorbeeld:

  * MVVM
  * Repository Pattern
  * Dependency Injection
  * Service Pattern
* Dependency Injection wordt gebruikt voor services en API-communicatie.
* Business logic staat niet direct in de UI-code (code-behind zo minimaal mogelijk).

## 3. Architectuur

* De gekozen architectuurstijl is consequent toegepast.
* De verantwoordelijkheden van:

  * Views
  * ViewModels
  * Services
  * Data/API laag
    zijn duidelijk gescheiden.
* API-calls verlopen via services of repositories.
* Hardware functionaliteit (GPS/sensors/notificaties) is geabstraheerd via interfaces/services.

## 4. Testen

### Unit tests

* Er zijn minimaal 3 betekenisvolle unit tests aanwezig.
* Unit tests testen business logic en niet alleen simpele getters/setters.
* Alle unit tests slagen succesvol.

### Voorbeelden

* ritberekening
* detectie stilstand
* klant suggestie logica

### UI / Device tests

* Er zijn minimaal 3 betekenisvolle UI/device running tests uitgevoerd.
* Tests zijn uitgevoerd op echte devices of emulators/simulators.

## 5. Gebruik van device functionaliteiten

* GPS-permissions worden correct afgehandeld.
* Background services functioneren stabiel.
* Bewegingsdetectie werkt zonder merkbare performanceproblemen.
* Notificaties verschijnen correct op beide platformen.
* De app gaat correct om met denied permissions of ontbrekende GPS-toegang.

## 6. API & Data

* De applicatie communiceert succesvol met de Web API.
* API-fouten worden netjes afgehandeld.
* JWT authenticatie werkt correct.
* Gegevens worden correct opgeslagen in MSSQL.
* Mock data of testdata kan gebruikt worden voor development/testing.

## 7. Performance & Stabiliteit

* De app crasht niet tijdens normaal gebruik.
* Navigatie tussen schermen werkt vloeiend.
* Locatie tracking veroorzaakt geen extreme batterijbelasting.
* Lange sessies (werkdag tracking) blijven stabiel functioneren.

## 8. Documentatie

* Er is een README of handleiding aanwezig met:

  * installatie instructies
  * build/run instructies
  * gebruikte technieken
  * architectuurkeuzes
  * uitleg van design patterns
* Er is beschreven hoe:

  * de app gestart wordt
  * de API gebruikt wordt
  * testen uitgevoerd worden
* Bekende beperkingen of toekomstige verbeteringen zijn benoemd.

## 9. Oplevering

* De applicatie kan succesvol worden gedemonstreerd.
* De applicatie draait werkend op Android én iOS.
* Alle user stories die “Done” zijn gemarkeerd voldoen aan deze Definition of Done.
* De applicatie is klaar voor beoordeling en portfolio-opname.
