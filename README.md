# 📘 Documentazione del Progetto

## 🚀 Tecnologie Utilizzate

### Frontend (Blazor WASM - PWA)
- Blazor WebAssembly con supporto offline (Progressive Web App)
- JWT per autenticazione custom

### Backend (ASP.NET Core)
- Appwrite per storage immagini
- ASP.NET Core Web API
- Entity Framework Core per accesso e migrazioni database
- Swagger per documentazione API
- Job scheduler interno per notifiche e task ricorrenti
---

## 🖼️ Frontend – Blazor WebAssembly (PWA)

### 📁 Struttura
```

Pages/       → Pagine Blazor
Shared/      → Componenti riutilizzabili
Services/    → Servizi per API REST (HttpClient)
wwwroot/     → Manifest e configurazione PWA

```

### 🔐 Autenticazione Custom (JWT)
- JWT salvato nei cookie del browser
- `CustomAuthProvider` implementa `AuthenticationStateProvider`
- **Login:** chiamata API backend, salvataggio token
- **HttpClient:** configurato per includere automaticamente JWT nelle richieste
- **Logout:** rimozione token e reset dello stato utente

---

## 🔙 Backend – ASP.NET Core

### 📁 Struttura
```

Controllers/     → API REST organizzate per dominio (auth, report, mappe)
Services/        → Logica applicativa (notifiche, PDF, Appwrite, ecc.)
Services/Jobs/   → Job schedulati (email, push, controlli periodici)

````

### 📄 Controller REST + Swagger
- Tutti i controller protetti da `[Authorize]`
- Documentazione API disponibile via Swagger (`/swagger`)

---

## 🛠️ Job Scheduler & Task Periodici

Sistema di scheduling integrato (o con Hangfire):

- ✅ **Check Report** – Dopo 7 giorni, verifica lo stato e invia notifica
- 🔔 **Push Notifications** – Invio notifiche verso client (PWA/mobile)
- 📧 **Email Notification** – Email automatica per report/aggiornamenti

## 🖼️ Appwrite – Gestione Bucket Immagini

* Upload immagini da backend via SDK o chiamata REST
* URL ottenuto e salvato in database
* Frontend mostra immagine via URL (pubblico o firmato)

---

## 🧱 Database & Entity Framework

* Database: PostgreSQL
* Utilizzo di **Entity Framework Core**
* Migrazioni gestite via CLI:

```bash
dotnet ef migrations add NomeMigrazione
dotnet ef database update
```

* Architettura a repository per separare accesso ai dati dalla logica

---

## 📦 Avvio del Progetto

```bash
# Backend
cd backend
dotnet restore
dotnet run --launch-profile "https" 

# Frontend (Blazor WASM)
cd frontend
dotnet restore
dotnet run --launch-profile "https"
```



