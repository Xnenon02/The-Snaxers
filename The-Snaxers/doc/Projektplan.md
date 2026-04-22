# 📓 The Snaxers — Projektplan 

## 🎯 Projektmål

Bygga, containerisera och deploya en lyxchokladapp på Azure med CI/CD, IaC, logging och monitorering — i enlighet med kursplanen för _Molnapplikationer fördjupning (40 yhp)_.

## 🎓 Koppling till lärandemål

| **Lärandemål**                           | **Hur vi uppfyller det**                                                                    |
| ---------------------------------------- | ------------------------------------------------------------------------------------------- |
| **Containerbaserad utveckling & deploy** | Docker + Azure Container Apps                                                               |
| **Infrastructure as Code (IaC)**         | Bicep-filer för alla Azure-resurser                                                         |
| **CI/CD-flöde**                          | GitHub Actions — bygg, test, deploy                                                         |
| **Logging & monitorering**               | Azure Application Insights (+ dokumenterat felsöknings-case)                                |
| **Säkerhet & secrets**                   | Azure Key Vault + Managed Identity (Passwordless)                                           |
| **Livscykelhantering**                   | Miljöer: dev / staging / prod                                                               |
| **AI-användning**                        | AI används genom hela projektet — dokumenteras löpande i AI-logg och sammanställs i rapport |

---

## 👥 **Rollfördelning

| Person   | User Stories                                                                   | Tekniskt ansvar                                                                  |
| -------- | ------------------------------------------------------------------------------ | -------------------------------------------------------------------------------- |
| Person 1 | US1 Favoritlista ✅, US4 Sökning✅                                               | Docker, Key Vault, Application Insights, Managed Identity, CI/CD, GitHub Actions |
| Person 2 | US2 (Galleri), US6 (Admin), US5 (Blob Storage), US8 (Vaurkorg), US10 (Flaggor) | Rollhantering                                                                    |
| Person 3 | US3 (Google OAuth), US7 (Miljöhantering)                                       | IaC (Bicep), CosmosDB, Azure-setup                                               |
> **Viktigt:** Alla hjälps åt med allt men har huvudansvar för sitt område. Alla ska kunna förklara _hela_ lösningen vid examination.

---

## 📋 User Stories

| **US**   | **Beskrivning**                                | **Person** | **Vecka** | **Status** |
| -------- | ---------------------------------------------- | ---------- | --------- | ---------- |
| **US1**  | Favoritlista (migreras till CosmosDB i v3)     | Person 1   | 1         | ✅ Klar     |
| **US2**  | Produktgalleri (grid, flaggor, REST Countries) | Person 2   | 1         | ✅ Klar     |
| **US3**  | Google OAuth + Key Vault                       | Person 3   | 2         | ⏳          |
| **US4**  | Sökning och filtrering                         | Person 1   | 1         | ✅ Klar     |
| **US5**  | Azure Blob Storage (produktbilder)             | Person 2   | 2         | ⏳          |
| **US6**  | Admin-panel (CRUD, rollskyddad)                | Person 2   | 2         | ⏳          |
| **US7**  | Miljöhantering dev/staging/prod                | Person 3   | 3         | ⏳          |
| **US8**  | Varukorg                                       | Person 2   | 4         | ⏳          |
| **US9**  | Dynamiska bilder Blob storage                  | Person 1   | 2         | ⏳          |
| **US10** | Info om länder                                 | Person 2   | 3         | ⏳          |

---

## 📅 Veckoplan (den är inte korrekt uppdaterad, vi kör vårt race)

### Vecka 1 ✅ (klar)

- **Gemensamt:**
    
    - MVC-app uppsatt med ASP.NET Core (.NET 10).
        
    - **Arkitektur:** Sätt upp en **3-tier arkitektur (MVC ➔ Service ➔ Repository)**. Controllern hanterar HTTP-anrop, Service-lagret (`IProductService`) hanterar affärslogik, och Repositoryt (`IProductRepository`) isolerar databasen..
        
    - Git-struktur: `main` → `develop` → `feature branches`.
        
    - US1 — Favoritlista klar (med SQLite).
        
    - AI-logg startad (löpande under hela projektet).
        
- **AI-logg ska innehålla:** _US/Task_ (Vad det gäller) ➔ _Verktyg_ (Vilken AI) ➔ _Prompt_ (Vad ni frågade) ➔ _Resultat_ (Vad ni fick ut och justerade).
    

### Vecka 2 — Produktgalleri, Modell & IaC-grund ✅ Klar

- **Gemensamt:** Uppdatera Product-modellen (`Brand`, `CocoaPercentage`, `Country`).
    
    - Ny migration + uppdatera testdata.
        
    - Beslut: App Service vs Container Apps (dokumentera skalbarhetsmotivering).
        
- **Person 1:** Stödja Person 2 med app-grund, code review.
    
- **Person 2:** US2 — Produktgalleri (grid, kort, placeholder-bild, REST Countries API) + enhetstester för produktlogik.
    
- **Person 3:** Sätt CosmosDB manuellt i portalen. **Skriv första IaC (Bicep)** för att sätta upp resursgrupp och CosmosDB (undvik manuella klick i Azure-portalen).
    

### Vecka 3 — Auth + Docker + Databas

- Person 1: Docker — Dockerfile + kör appen i container lokalt + taggningsstrategi för images
    
- Person 2: GitHub Actions CI/CD-pipeline — bygg, testa och deploya till Azure
    
- Person 3: Google OAuth + Key Vault + migrera SQLite → CosmosDB. Börja dokumentera kvarstående manuella steg för att automatisera så mycket som möjligt i Bicep. 
    

> ⚠️ _När CosmosDB är på plats är US1 fullt klar enligt AC4._

### Vecka 4 — CI/CD + Azure Deploy

- Person 1: Koppla Key Vault i produktion + konfigurera Managed Identity (Passwordless koppling till CosmosDB och Key Vault)
    
- Person 2: GitHub Actions pipeline — bygg, testa (pipeline kör enhetstester automatiskt) och deploya till Azure
    
- Person 3: Azure Container Registry + App Service/Container Apps + ACR retention-policy (rensa gamla images) + testa Google OAuth mot Azure
    

### Vecka 5 — IaC + Admin + Sökning

- Person 1: ~~US4 — Sökning och filtrering av produkter~~ → US4 klar (vecka 2), stödja + code review
    
- Person 2: US6 — Admin-panel (CRUD för produkter, rollskyddad med `[Authorize(Roles="Admin")]`)
    
- Person 3: Expandera IaC med Bicep — parametriserade miljöer (`dev`/`staging`/`prod`) för CosmosDB, Key Vault och App Service/ACA
    

### Vecka 6 — Blob Storage + Logging + Felsökning

- Person 1: Application Insights — logging i controllers, alerts, dashboard. Skapa ett konkret felsöknings-case (t.ex. en `/api/test-error` endpoint som kastar ett fel) och dokumentera spårningen i App Insights för rapporten
    
- Person 2: US5 — Azure Blob Storage för produktbilder
    
- Person 3: US7 — Miljöhantering dev/staging/prod med olika konfigurationer
    

### Vecka 7 — Polish + Skalbarhet + Livscykel

- **Alla tillsammans:**
    
    - Optimera CI/CD-pipeline och responsiv design.
        
    - Buggar och code review.
        
    - Skalbarhetsdokument: autoscaling-config, motivering till val av Container Apps.
        
    - Livscykel-review: image-taggning, ACR-städning, zero-downtime deploy.
        
    - AI-logg sammanställs — alla tre skriver sin del.
        
    - Verifiera att alla lärandemål är täckta.
        

### Vecka 8 — Rapport + Presentation

- **Alla tillsammans:**
    
    - Dokumentation klar.
        
    - Rapport inkl. AI-användning (hur AI användes, konkreta exempel, Managed Identity).
        
    - **Lyft fram i rapporten:** Appen är "Passwordless" (Managed Identity) och 100% IaC (Bicep).
        
    - Presentation av hela lösningen.
        

---

## 🔧 Tekniska Tasks (ej user stories)

| **Task**                                                                       | **Person** | **Vecka** | **Kursplansmål**   |
| ------------------------------------------------------------------------------ | ---------- | --------- | ------------------ |
| Uppdatera Product-modellen & sätt upp 3-tier (Service + Repository interfaces) | Alla       | 1-2       | —                  |
| Skapa grundläggande IaC (Bicep)                                                | Person 3   | 2         | IaC                |
| Enhetstester (produktlogik)                                                    | Person 2   | 2         | —                  |
| Docker + Dockerfile                                                            | Person 1   | 3         | Containerisering   |
| Taggningsstrategi för images                                                   | Person 1   | 3         | Livscykelhantering |
| Migrera till CosmosDB (EF Core Cosmos Provider)                                | Person 3   | 3         | Molntjänster       |
| GitHub Actions CI/CD (inkl. testkörning)                                       | Person 2   | 4         | CI/CD              |
| ACR + retention-policy                                                         | Person 3   | 4         | Livscykelhantering |
| Expandera IaC (parametriserade miljöer)                                        | Person 3   | 5         | IaC                |
| Rollhantering Admin/User                                                       | Person 2   | 5         | Säkerhet           |
| Konfigurera Managed Identity                                                   | Person 1   | 3-5       | Säkerhet           |
| App Insights + dokumenterat felsöknings-case                                   | Person 1   | 6         | Logging/Felsökning |
| Skalbarhetsdokument + autoscaling-config                                       | Alla       | 7         | Skalbarhet         |
| Livscykel-review (image-städning, zero-downtime)                               | Alla       | 7         | Livscykelhantering |

---

## 🏗️ Teknisk stack vid kursslut

- **CI/CD:** GitHub Actions
    
- **Container:** Azure Container Registry ➔ Azure Container Apps
    
- **Databas:** Azure CosmosDB (via EF Core Cosmos Provider)
    
- **Säkerhet:** Azure Key Vault + **Managed Identity (Passwordless)**
    
- **Filer:** Azure Blob Storage (produktbilder)
    
- **Monitorering:** Azure Application Insights
    

---

## ⚠️ Viktiga beslut tagna / att ta

1. **Säkerhet (v3):** Managed Identity används för anslutningar för att slippa hantera connection strings.
?
    
2. **Drift (v2):** Besluta Container Apps vs App Service — dokumentera motiveringen direkt.
?
    

---

## 📝 Acceptanskriterier (AC)

### US1 — Favoritlista - Jag

Som en chokladälskare vill jag kunna spara specifika produkter i en favoritlista så att jag enkelt kan hitta tillbaka till mina lyxval.

- **AC1:** Användaren kan klicka på en hjärtikon för att spara produkten.
    
- **AC2:** Visuell feedback (🤍 → ❤️) när produkten sparas.
    
- **AC3:** Kan tas bort via hjärtikon eller inifrån listvyn.
    
- **AC4:** Persistens: Sparas i CosmosDB (vecka 3) och överlever utloggning.
    
- **AC5:** Navigering "❤️ Mina favoriter" syns endast för inloggade.
    
- **AC6:** Tomt läge uppmanar användaren att utforska sortimentet.
    

### US2 — Lyxchokladgalleri

Som en besökare vill jag kunna bläddra i ett galleri av lyxchoklad.

- **AC1:** Responsivt grid med namn, märke, kakaohalt och bild.
    
- **AC2:** Placeholder-bild visas om bild saknas.
    
- **AC3:** Landsinformation via REST Countries API (flagga visas).
    
- **AC4:** Tomt läge visar "Ingen choklad hittades för tillfället".
    

### US3 — Google OAuth + Key Vault

Som en användare vill jag kunna logga in smidigt med mitt Google-konto.

- **AC1:** Knapp "Logga in med Google" omdirigerar till Google och tillbaka.
    
- **AC2:** Profil skapas i CosmosDB vid första inloggning.
    
- **AC3:** Säkerhet: ClientID/ClientSecret hämtas från Key Vault (prod) eller User Secrets (dev). Appen använder Managed Identity.
    
- **AC4:** Endast inloggade kan nå `/favorites` (redirect till login om oinloggad).
    
- **AC5:** Inloggad användares namn/bild visas i navigationen.
    

### US4 — Sökning och filtrering - Jag

Som en besökare vill jag kunna söka och filtrera produkter.

- **AC1:** Sökfält filtrerar direkt på produktnamn.
    
- **AC2:** Filtrering på kakaohalt via fördefinierat filter (t.ex. "70% eller mer").
    
- **AC3:** Om inget hittas visas "Inga produkter matchade din sökning".
    

### US5 — Azure Blob Storage

Som en administratör vill jag ladda upp produktbilder i molnet.

- **AC1:** Uppladdning via admin-panel sparas i Blob Storage och visas på kortet.
    
- **AC2:** Placeholder visas om bild saknas.
    
- **AC3:** Validering av filtyp: Endast bildfiler (jpg, png, webp) tillåts.
    

### US6 — Admin-panel (CRUD)

Som en administratör vill jag kunna hantera produkter i ett gränssnitt.

- **AC1:** Skapa ny produkt (namn, varumärke, kakaoandel, land).
    
- **AC2:** Uppdatera befintlig produkt.
    
- **AC3:** Ta bort produkt (med bekräftelse).
    
- **AC4:** Rollskydd: Kräver rollen `Admin`. `User` får "Behörighet saknas" / redirect.
    

### US7 — Miljöhantering dev/staging/prod

Som en utvecklare vill jag att applikationen använder separata konfigurationer per miljö.

- **AC1:** `Development` använder lokal SQLite och User Secrets (inga produktionsresurser).
    
- **AC2:** `Staging` använder staging-instanser av CosmosDB, Key Vault och App Insights.
    
- **AC3:** `Production` använder produktionsinstanserna.
    
- **AC4:** Om miljövariabeln saknas loggas ett fel och appen startar inte.


### US8: Varukorg
Som en kund vill jag kunna lägga produkter i en varukorg så att jag kan samla mina inköp innan jag betalar.

- AC1: Klick på en "Lägg i varukorg"-knapp lägger till vald produkt i varukorgen. 
     
- AC2: Varukorgsvyn visar en lista med de valda produkterna (bild, namn, styckpris och valt antal). - AC3: Varukorgen räknar automatiskt ut och visar totalsumman för hela beställningen. 
    
- AC4: Kunden kan öka, minska eller helt ta bort produkter direkt i varukorgen (och totalsumman uppdateras direkt). 
    
- AC5: En varukorgsikon i huvudmenyn visar alltid en siffra på aktuellt antal varor i korgen.
    

### US9 — Dynamisk och säker bildhantering för chokladprodukter

**Som en** administratör **vill jag** kunna ladda upp, hantera och radera unika produktbilder för varje chokladtyp via ett säkert gränssnitt, **så att** kunderna får en korrekt, högkvalitativ visuell representation av sortimentet och galleriet förblir dynamiskt.

- **AC1: Säker bilduppladdning.** Via Admin-panelen (US6) kan jag ladda upp en ny bild för en specifik produkt. Bilden sparas i Azure Blob Storage (US5) och URL:en sparas automatiskt på produkten i CosmosDB.
    
- **AC2: Visuell fallback.** Om en produkt saknar en uppladdad bild, eller om länken är bruten, visas automatiskt en lokal standardbild (placeholder-choco.png) i produktgalleriet (US2).
    
- **AC3: Säkerhet och validering.** Systemet accepterar endast giltiga bildformat (.jpg, .png, .webp) med en maximal filstorlek på 2 MB. Otillåtna filer avvisas med ett tydligt felmeddelande.
    
- **AC4: Molnstädning (Livscykel).** Om en chokladprodukt raderas helt från databasen (via Admin-panelen), raderas även tillhörande bildfil permanent från Azure Blob Storage för att undvika onödiga lagringskostnader.
    
- **AC5: Dynamisk uppdatering.** Om jag laddar upp en ny bild på en befintlig produkt skrivs den gamla bilden över eller raderas, och det nya utseendet reflekteras direkt på sidan.

### US10 - Flaggor

Som användare vill jag kunna klicka på en flagga och få mer info om landet chokladen kommer ifrån

- **AC1: Interaktiv flagga** Varje produkt i galleriet som har ett ursprungsland ska visa landets flagga (som ikon eller emoji). Flaggan ska vara klickbar.
    
- **AC2: Informationsvy (Modal/Popup)** Vid klick på flaggan ska en modal eller en snygg popup dyka upp (utan att användaren lämnar galleriet).
    
- **AC3: Dynamisk information** Informationen i modalen ska innehålla minst:
    
    - Landets namn.
        
    - En kort text om landets chokladtradition eller kakaoproduktion (kan hämtas via ett enkelt API eller en hårdkodad "CountryService").
        
- **AC4: Felhantering (Fallback)** Om information om ett specifikt land saknas i systemet, ska en vänlig text visas (t.ex. _"Vi fyller på med information om [Land] inom kort!"_) istället för att modalen är tom eller kraschar.
    
- **AC5: Stängningsfunktion** Användaren ska enkelt kunna stänga modalen via ett "X" eller genom att klicka utanför rutan för att återgå till galleriet.


--- 

### 🕵️‍♂️ Analys av arbetsbördan - Detta stämmer ej mer då vi ändrat lite, se ovan

**Person 1: Säkerhet & Övervakning**

- **Kod:** Favoritlista, Dynamisk bildhantering, Unit tester och Sökning/Filtrering.
    
- **Moln/Tech:** Docker, Key Vault, Managed Identity, Application Insights.
    
- **Varför det är tungt:** Att få Docker-containern att snurra korrekt i molnet + Att sätta upp "Passwordless" infrastruktur mot Azure Key Vault och CosmosDB via Managed Identity är klurigt och kräver djup förståelse för Azures behörighetssystem (RBAC). Du ansvarar för att täppa till alla säkerhetshål och bygga spårbarhet (App Insights) så att teamet kan hitta fel i produktion. 
    

**Person 2: DevOps & Filer**

- **Kod:** Produktgalleri, Varukorg, Admin-panel (CRUD), integrera Blob Storage.
    
- **Moln/Tech:** GitHub Actions (CI/CD), Rollhantering.
    
- **Varför det är tungt:** CI/CD-pipelines är ökända för att ta tid ("varför bygger den lokalt men inte i GitHub Actions?!"). Hantera filströmmar (bilduppladdning till Blob Storage) och rollhantering, är ett stort och viktigt lass.
    

**Person 3: Infrastruktur & Databas (Den tunga motorn)**

- **Kod:** Miljöhantering och **Google OAuth (US3)**.
    
- **Moln/Tech:** IaC (Bicep) för hela Azure-miljön (3 miljöer!), migrera databasen till CosmosDB med EF Core, sätta upp ACR-policy.
    
- **Varför det är tungt:** Person 3 har nu tagit på sig ett enormt lass. Att skriva Bicep-kod som dynamiskt skapar resurser för dev, staging och prod kombinerat med att sätta upp Googles inloggningsflöde och migrera till CosmosDB gör detta till den tyngsta rollen i projektet. Person 3 bygger hela det fundament som appen vilar på.
    

---

### Slutsats - Detta stämmer ej mer då vi ändrat lite, se ovan

Ni har lyckats skapa tre helt unika specialistroller:

1. **Security & Monitoring Engineer** (Person 1)
    
2. **DevOps & Fullstack Engineer** (Person 2)
    
3. **Cloud Infrastructure & Data Engineer** (Person 3)


![[Hemsidan Snaxers.png]]

