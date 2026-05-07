
# Voorstel .NET MAUI App

## Vraag 1

### Wie zijn de beoogde gebruikers van de .NET MAUI app?

**Beoogde gebruikers:**
De app is bedoeld voor klanten van Time On die veel onderweg zijn naar klanten, zoals consultants, monteurs of accountmanagers.

### Wat is het doel (core functionaliteit) van de .NET MAUI app?

**Doel van de app:**
Het doel van de app is het automatisch registreren van gereden kilometers en bezochte klantlocaties gedurende de werkdag. De app detecteert rijbewegingen en stops, slaat locaties op en helpt de gebruiker bij het eenvoudig bijhouden van rittenadministratie en klantbezoeken.

### Belangrijke functionaliteit

* Automatische kilometerregistratie
* Detectie van klantlocaties (bij stilstand)
* Overzicht van dagritten
* Slimme klantsuggesties op basis van locatie

---

# Vraag 2

## Welke userstories wil je uitwerken in de app?

1. Als gebruiker wil ik kunnen inloggen zodat mijn gegevens veilig opgeslagen worden.
2. Als gebruiker wil ik mijn werkdag kunnen starten zodat mijn ritten automatisch worden bijgehouden.
3. Als gebruiker wil ik dat de app automatisch detecteert wanneer ik aan het rijden ben zodat kilometers geregistreerd worden.
4. Als gebruiker wil ik dat de app stopt met kilometerregistratie wanneer ik stilsta zodat alleen relevante kilometers worden opgeslagen.
5. Als gebruiker wil ik dat een locatie wordt opgeslagen wanneer ik langer dan x minuten stilsta zodat klantbezoeken automatisch geregistreerd worden.
6. Als gebruiker wil ik een overzicht van mijn gereden kilometers per dag zodat ik mijn rittenadministratie kan bijhouden.
7. Als gebruiker wil ik een lijst zien van bezochte klantlocaties zodat ik mijn werkdag kan analyseren.
8. Als gebruiker wil ik automatische klantsuggesties krijgen op basis van mijn locatie zodat ik snel een klant kan koppelen aan een bezoek.
9. Als gebruiker wil ik meldingen ontvangen wanneer ik weer begin met rijden zodat ik weet dat de kilometerregistratie actief is.
10. Als gebruiker wil ik mijn ritten kunnen corrigeren of aanpassen zodat fouten hersteld kunnen worden.
11. Als gebruiker wil ik dat mijn data lokaal afgeschermd word en alleen de losse ritten / boekingen opgeslagen worden zodat ik mijn privacy behoud.

---

# Vraag 3

## Beschrijf de functionaliteit van de web api die van je .NET MAUI app gaat benutten.

De app maakt gebruik van een (eigen) Web API die verantwoordelijk is voor het opslaan en ophalen van gegevens. Alle entities komen vanuit Time On zodat integreren in de applicatie later zo makkelijk mogelijk gaat.

### Functionaliteit van de Web API

* Authenticatie (login/logout, JWT tokens)
* Opslaan van ritgegevens (starttijd, eindtijd, kilometers)
* Opslaan van locaties (GPS-coördinaten, adres)
* Ophalen van klantgegevens
* Koppelen van locaties aan klanten
* Ophalen van dagoverzichten

### Database

* Database: MSSQL
* Tabellen zoals:

  * Users
  * Trips
  * Locations
  * Customers

### Mock mogelijkheid

Er kan gebruik gemaakt worden van onze eigen test database voor klant data. Dit zal los geïmporteerd worden in de database.

---

# Vraag 4

## Beschrijf of, en zo ja hoe je gebruik maakt van de hardware van mobile device.

De app maakt actief gebruik van hardware van het mobiele apparaat.

### Gebruikte features

* **GPS (Location services)**
  Voor het tracken van locatie, snelheid en beweging.

* **Background services**
  De app blijft locatie tracken terwijl deze op de achtergrond draait.

* **Sensors (accelerometer / motion detection)**
  Om te detecteren of de gebruiker beweegt (rijden vs. stilstand).

* **Notifications (push/local notifications)**
  Voor meldingen zoals:

  * Start rijden
  * Stop gedetecteerd
  * Suggestie klant

### Platformen

De app wordt ontwikkeld voor:

* Android
* iOS
