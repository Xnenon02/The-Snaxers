# 📓 The Snaxers — Projektplan 

## 🎯 Projektmål

Bygga, containerisera och deploya en lyxchokladapp på Azure med CI/CD, IaC, logging och monitorering — i enlighet med kursplanen för _Molnapplikationer fördjupning (40 yhp)_.

---

## Status 23 april: 

**(Person 1):**
- ✅ US1 Favoritlista 
- ✅ US4 Sökning 
- ✅ US9 Dynamiska bilder 
- ✅ Docker + Dockerfile + multi-platform
- ✅ Docker Compose + .env secret management
- ✅ CI/CD (GitHub Actions — build + test)
- ✅ REST API + Scalar + ApiKeyFilter
- ✅ Health checks
- ✅ Strukturerad logging i alla controllers
- ✅ CountryService caching + timeout + ILogger
- ✅ Bicep security & monitoring (förberedelse)
- ✅ Keyvault & Application Insights (förberedelser)
- ✅ XUnit Countryservice TDD
- ⏳ Application Insights mot riktig Azure + felsöknings-case — väntar på Tom
- ⏳ Managed Identity kopplad till Container App — väntar på Tom

**(Person 2):**

- ✅ US2 Produktgalleri
- ✅ US5 Blob Storage
- ✅ US6 Admin-panel
- ✅ Rollhantering Admin/User
- ⏳ US8 Varukorg — ej påbörjat
- ⏳ US10 Flaggor med modal — PR 
- ⏳ Deploy-steg i CI/CD pipeline (ACR) — väntar på Tom

**(Person 3):**

- ✅ US3 Google OAuth
- ✅ US7 Miljöhantering dev/staging/prod
- ✅ CosmosDB migration
- ⏳ ACR + Azure Container Apps setup
- ⏳ Expandera Bicep med parametriserade miljöer

---

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

## 👥 Rollfördelning

| Person   | User Stories                                                                           | Tekniskt ansvar                                                                                   |
| -------- | -------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Person 1 | US1 Favoritlista ✅, US4 Sökning ✅, US9 Dynamiska bilder ✅                              | Docker ✅, CI/CD build+test ✅, REST API ✅, Key Vault ⏳, Application Insights ⏳, Managed Identity ⏳ |
| Person 2 | US2 (Galleri) ✅, US6 (Admin) ✅, US5 (Blob Storage) ✅, US8 (Varukorg) ⏳, US10 (Flaggor) | Blob Storage ✅, Rollhantering ✅, Deploy-steg i pipeline (ACR) ⏳                                   |
| Person 3 | US3 (Google OAuth) ✅, US7 (Miljöhantering) ✅                                           | IaC (Bicep) ✅, CosmosDB ✅, Azure-setup ⏳                                                          |

> **Viktigt:** Alla hjälps åt med allt men har huvudansvar för sitt område. Alla ska kunna förklara _hela_ lösningen vid examination.

---

## 📋 User Stories

| **US**   | **Beskrivning**                                | **Person** | **Vecka** | **Status** |
| -------- | ---------------------------------------------- | ---------- | --------- | ---------- |
| **US1**  | Favoritlista (migreras till CosmosDB i v3)     | Person 1   | 1         | ✅ Klar     |
| **US2**  | Produktgalleri (grid, flaggor, REST Countries) | Person 2   | 1         | ✅ Klar     |
| **US3**  | Google OAuth + Key Vault                       | Person 3   | 2         | ✅ Klar     |
| **US4**  | Sökning och filtrering                         | Person 1   | 1         | ✅ Klar     |
| **US5**  | Azure Blob Storage (produktbilder)             | Person 2   | 2         | ✅ Klar     |
| **US6**  | Admin-panel (CRUD, rollskyddad)                | Person 2   | 2         | ✅ Klar     |
| **US7**  | Miljöhantering dev/staging/prod                | Person 3   | 3         | ✅ Klar     |
| **US8**  | Varukorg                                       | Person 2   | 4         | ⏳          |
| **US9**  | Dynamiska bilder Blob storage                  | Person 1   | 2         | ✅ Klar     |
| **US10** | Info om länder med modal                       | Person 2   | 3         | ⏳          |

---

## 📅 Veckoplan (vi kör vårt race)

### Vecka 1 ✅ (klar)

- **Gemensamt:**
    - MVC-app uppsatt med ASP.NET Core (.NET 10).
    - **Arkitektur:** 3-tier arkitektur (MVC ➔ Service ➔ Repository).
    - Git-struktur: `main` → `develop` → `feature branches`.
    - US1 — Favoritlista klar (med SQLite).
    - AI-logg startad (löpande under hela projektet).

- **AI-logg ska innehålla:** _US/Task_ ➔ _Verktyg_ ➔ _Prompt_ ➔ _Resultat_

### Vecka 2 — Produktgalleri, Modell & IaC-grund ✅ Klar

- **Gemensamt:** Uppdatera Product-modellen (`Brand`, `CocoaPercentage`, `Country`).
- **Person 1:** US4 Sökning & filtrering, US9 Dynamiska bilder, Docker + Dockerfile, code review.
- **Person 2:** US2 Produktgalleri (grid, kort, placeholder-bild, REST Countries API).
- **Person 3:** CosmosDB setup, första IaC (Bicep).

### Vecka 3 — Auth + Docker + Databas ✅

- Person 1: Docker Compose + .env secret management, Bicep security & monitoring, enhetstester CountryService
- Person 2: Rollhantering Admin/User
- Person 3: Google OAuth + Key Vault + migrera SQLite → CosmosDB, Miljöhantering US7

> ⚠️ _När CosmosDB är på plats är US1 fullt klar enligt AC4._

### Vecka 4 — CI/CD + Azure Deploy ⏳

- Person 1: GitHub Actions CI/CD (build + test) ✅, REST API + Scalar ✅, ACR-deploy ⏳, Managed Identity ⏳
- Person 2: US6 Admin-panel ✅, US5 Blob Storage ✅, Deploy-steg pipeline ⏳
- Person 3: ACR + Container Apps setup ⏳, Expandera Bicep ⏳

### Vecka 5 — Azure + Polish ⏳

- Person 1: Key Vault i produktion, Managed Identity klar, Application Insights
- Person 2: US8 Varukorg, US10 Flaggor med modal
- Person 3: Parametriserade Bicep-miljöer (dev/staging/prod)

### Vecka 6 — Logging + Felsökning ⏳

- Person 1: Application Insights dashboard + dokumenterat felsöknings-case (`/api/test-error`)
- Person 2: Finslipa US8 + US10
- Person 3: Verifiera miljöer i Azure

### Vecka 7 — Polish + Skalbarhet + Livscykel ⏳

- **Alla tillsammans:**
    - Optimera CI/CD-pipeline och responsiv design.
    - Buggar och code review.
    - Skalbarhetsdokument: autoscaling-config, motivering till val av Container Apps.
    - Livscykel-review: image-taggning, ACR-städning, zero-downtime deploy.
    - AI-logg sammanställs — alla tre skriver sin del.
    - Verifiera att alla lärandemål är täckta.

### Vecka 8 — Rapport + Presentation ⏳

- **Alla tillsammans:**
    - Dokumentation klar.
    - Rapport inkl. AI-användning.
    - **Lyft fram i rapporten:** Appen är "Passwordless" (Managed Identity) och 100% IaC (Bicep).
    - Presentation av hela lösningen.

---

## 🔧 Tekniska Tasks (ej user stories)

| **Task**                                                                       | **Person** | **Vecka** | **Status** | **Kursplansmål**   |
| ------------------------------------------------------------------------------ | ---------- | --------- | ---------- | ------------------ |
| Uppdatera Product-modellen & sätt upp 3-tier                                   | Alla       | 1-2       | ✅          | —                  |
| Skapa grundläggande IaC (Bicep)                                                | Person 3   | 2         | ✅          | IaC                |
| Enhetstester (CountryService)                                                  | Person 1   | 2         | ✅          | —                  |
| Docker + Dockerfile + multi-platform                                           | Person 1   | 3         | ✅          | Containerisering   |
| Taggningsstrategi för images                                                   | Person 1   | 3         | ✅          | Livscykelhantering |
| Docker Compose + .env secret management                                        | Person 1   | 3         | ✅          | Containerisering   |
| Migrera till CosmosDB (EF Core Cosmos Provider)                                | Person 3   | 3         | ✅          | Molntjänster       |
| CountryService caching + 3s timeout + ILogger                                 | Person 1   | 4         | ✅          | —                  |
| GitHub Actions CI/CD (build + test)                                            | Person 1   | 4         | ✅          | CI/CD              |
| REST API + Scalar + ApiKeyFilter                                               | Person 1   | 4         | ✅          | REST API           |
| Health checks                                                                  | Person 1   | 4         | ✅          | Monitorering       |
| Strukturerad logging i alla controllers                                        | Person 1   | 4         | ✅          | Logging            |
| Rollhantering Admin/User                                                       | Person 2   | 3         | ✅          | Säkerhet           |
| ACR + retention-policy                                                         | Person 3   | 4         | ⏳          | Livscykelhantering |
| Deploy-steg i CI/CD pipeline (ACR push + Container Apps)                      | Person 2   | 4-5       | ⏳          | CI/CD              |
| Expandera IaC (parametriserade miljöer)                                        | Person 3   | 5         | ⏳          | IaC                |
| Konfigurera Managed Identity                                                   | Person 1   | 5         | ⏳          | Säkerhet           |
| App Insights + dokumenterat felsöknings-case                                   | Person 1   | 6         | ⏳          | Logging/Felsökning |
| Skalbarhetsdokument + autoscaling-config                                       | Alla       | 7         | ⏳          | Skalbarhet         |
| Livscykel-review (image-städning, zero-downtime)                               | Alla       | 7         | ⏳          | Livscykelhantering |

---

## 🏗️ Teknisk stack vid kursslut

- **CI/CD:** GitHub Actions
- **Container:** Azure Container Registry ➔ Azure Container Apps
- **Databas:** Azure CosmosDB (via EF Core Cosmos Provider)
- **Säkerhet:** Azure Key Vault + **Managed Identity (Passwordless)**
- **Filer:** Azure Blob Storage (produktbilder)
- **Monitorering:** Azure Application Insights
- **Rest API:** Möjliggör att externa system eller mobilappar kan hämta produktdata programmatiskt utan att rendera HTML — t.ex. en mobilapp för The Snaxers eller en extern partner som vill integrera sortimentet.

---

## 📝 Acceptanskriterier (AC)

### US1 — Favoritlista — Person 1

Som en chokladälskare vill jag kunna spara specifika produkter i en favoritlista så att jag enkelt kan hitta tillbaka till mina lyxval.

- **AC1:** Användaren kan klicka på en hjärtikon för att spara produkten.
- **AC2:** Visuell feedback (🤍 → ❤️) när produkten sparas.
- **AC3:** Kan tas bort via hjärtikon eller inifrån listvyn.
- **AC4:** Persistens: Sparas i CosmosDB och överlever utloggning.
- **AC5:** Navigering "❤️ Mina favoriter" syns endast för inloggade.
- **AC6:** Tomt läge uppmanar användaren att utforska sortimentet.

### US2 — Lyxchokladgalleri — Person 2

Som en besökare vill jag kunna bläddra i ett galleri av lyxchoklad.

- **AC1:** Responsivt grid med namn, märke, kakaohalt och bild.
- **AC2:** Placeholder-bild visas om bild saknas.
- **AC3:** Landsinformation via REST Countries API (flagga visas).
- **AC4:** Tomt läge visar "Ingen choklad hittades för tillfället".

### US3 — Google OAuth + Key Vault — Person 3

Som en användare vill jag kunna logga in smidigt med mitt Google-konto.

- **AC1:** Knapp "Logga in med Google" omdirigerar till Google och tillbaka.
- **AC2:** Profil skapas vid första inloggning (SQLite via Identity — medvetet val, CosmosDB används för produkter/favoriter).
- **AC3:** ClientID/ClientSecret hämtas från Key Vault (prod) eller User Secrets (dev).
- **AC4:** Endast inloggade kan nå `/favorites` (redirect till login om oinloggad).
- **AC5:** Inloggad användares namn/bild visas i navigationen.

### US4 — Sökning och filtrering — Person 1

Som en besökare vill jag kunna söka och filtrera produkter.

- **AC1:** Sökfält filtrerar direkt på produktnamn.
- **AC2:** Filtrering på kakaohalt via fördefinierat filter (t.ex. "70% eller mer").
- **AC3:** Om inget hittas visas "Inga produkter matchade din sökning".
- **AC4:** Sökfilter bevaras när man sparar/tar bort favoriter.

### US5 — Azure Blob Storage — Person 2

Som en administratör vill jag ladda upp produktbilder i molnet.

- **AC1:** Uppladdning via admin-panel sparas i Blob Storage och visas på kortet.
- **AC2:** Placeholder visas om bild saknas.
- **AC3:** Validering av filtyp: Endast bildfiler (jpg, png, webp) tillåts.

### US6 — Admin-panel (CRUD) — Person 2

Som en administratör vill jag kunna hantera produkter i ett gränssnitt.

- **AC1:** Skapa ny produkt (namn, varumärke, kakaoandel, land).
- **AC2:** Uppdatera befintlig produkt.
- **AC3:** Ta bort produkt (med bekräftelse).
- **AC4:** Rollskydd: Kräver rollen `Admin`. `User` får "Behörighet saknas" / redirect.

### US7 — Miljöhantering dev/staging/prod — Person 3

Som en utvecklare vill jag att applikationen använder separata konfigurationer per miljö.

- **AC1:** `Development` använder lokal SQLite och User Secrets (inga produktionsresurser).
- **AC2:** `Staging` använder staging-instanser av CosmosDB, Key Vault och App Insights.
- **AC3:** `Production` använder produktionsinstanserna.
- **AC4:** Om miljövariabeln saknas loggas ett fel och appen startar inte.

### US8 — Varukorg — Person 2

Som en kund vill jag kunna lägga produkter i en varukorg så att jag kan samla mina inköp innan jag betalar.

- **AC1:** Klick på "Lägg i varukorg" lägger till vald produkt i varukorgen.
- **AC2:** Varukorgsvyn visar lista med valda produkter (bild, namn, styckpris, antal).
- **AC3:** Varukorgen räknar automatiskt ut och visar totalsumman.
- **AC4:** Kunden kan öka, minska eller ta bort produkter direkt i varukorgen.
- **AC5:** En varukorgsikon i huvudmenyn visar aktuellt antal varor.

### US9 — Dynamisk och säker bildhantering — Person 1

Som en administratör vill jag kunna ladda upp, hantera och radera unika produktbilder via ett säkert gränssnitt.

- **AC1:** Bild sparas i Azure Blob Storage och URL sparas automatiskt i CosmosDB.
- **AC2:** Placeholder visas om bild saknas eller länk är bruten.
- **AC3:** Endast .jpg, .png, .webp tillåts, max 2 MB.
- **AC4:** Blob raderas när produkt raderas (undviker orphaned blobs).
- **AC5:** Gammal blob raderas när ny bild laddas upp.

### US10 — Info om länder med modal — Person 2

Som användare vill jag kunna klicka på en flagga och få mer info om landet chokladen kommer ifrån.

- **AC1:** Landets flagga visas på varje produkt och är klickbar.
- **AC2:** Modal/popup öppnas utan att användaren lämnar galleriet.
- **AC3:** Modalen visar landets namn och kort text om chokladtradition.
- **AC4:** Fallback-text visas om information saknas.
- **AC5:** Modalen stängs via "X" eller klick utanför.

---

## 🕵️‍♂️ Analys av arbetsbördan

**Person 1: Security & Monitoring Engineer**

- **Kod:** US1 Favoritlista, US4 Sökning & filtrering, US9 Dynamiska bilder, Enhetstester, REST API, bugfix sökning+favoriter.
- **Moln/Tech:** Docker + Docker Compose, GitHub Actions CI/CD (build+test), CountryService caching, Key Vault, Managed Identity, Application Insights, ACR-deploy.
- **Varför det är tungt:** Ansvarar för hela säkerhetsarkitekturen (Passwordless/Managed Identity), containerisering mot riktig Azure, och CI/CD-pipeline. Kräver djup förståelse för Azures RBAC-system och DevOps-flöden.  Magic bytes-validering och livscykelhantering är tekniskt krävande.

**Person 2: Fullstack & DevOps Engineer**

- **Kod:** US2 Produktgalleri, US5 Blob Storage, US6 Admin-panel, US8 Varukorg, US10 Flaggor med modal.
- **Moln/Tech:** Rollhantering Admin/User, Deploy-steg i CI/CD pipeline (ACR).
- **Varför det är tungt:** Blob Storage-integrationen är tekniskt krävande. Admin-panelen med rollskydd och CRUD mot CosmosDB är ett stort kodmässigt bidrag. US8 och US10 återstår.

**Person 3: Cloud Infrastructure & Data Engineer**

- **Kod:** US3 Google OAuth, US7 Miljöhantering.
- **Moln/Tech:** IaC (Bicep) för hela Azure-miljön, CosmosDB migration, ACR + Container Apps setup, parametriserade miljöer (dev/staging/prod).
- **Varför det är tungt:** Bicep-kod för tre miljöer kombinerat med CosmosDB-migrering och Google OAuth-flödet gör detta till den infrastrukturellt tyngsta rollen. Person 3 bygger det fundament hela appen vilar på.

---

### Slutsats

Tre tydliga specialistroller med god arbetsfördelning:

1. **Security & Monitoring Engineer** (Person 1)
2. **Fullstack & DevOps Engineer** (Person 2)
3. **Cloud Infrastructure & Data Engineer** (Person 3)

---

**Healthcheck:** [http://localhost:5065/health](http://localhost:5065/health)

![[Hemsidan Snaxers.png]]

