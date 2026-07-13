# TailorTalent (AIJobHunter)

AI-powered resume tailoring platform. Three tiers:

| Tier | Tech | Port | Location |
|---|---|---|---|
| Frontend | Angular 19 + Material | 4200 | `src/TailorTalent.Web` |
| Backend API | ASP.NET Core 9 + EF Core | 5111 | `src/TailorTalent.Api` |
| AI service | Python FastAPI | 8000 | `ai-service` |

## Configuration & connection strings

### Backend API (`src/TailorTalent.Api/appsettings.json`)

The database provider is switchable — no code change needed:

```jsonc
"Database": { "Provider": "Sqlite" },        // "Sqlite" (default) or "SqlServer"
"ConnectionStrings": {
  "Sqlite":    "Data Source=TailorTalent.db",
  "SqlServer": "Server=localhost;Database=TailorTalent;User Id=tailortalent_app;Password=...;TrustServerCertificate=True;Encrypt=True"
},
"AiService": { "BaseUrl": "http://localhost:8000" }   // FastAPI AI service
```

Resolution order: `ConnectionStrings:<Provider>` → `ConnectionStrings:DefaultConnection` → SQLite file fallback.
Any setting can be overridden with environment variables, e.g.:

```
set Database__Provider=SqlServer
set ConnectionStrings__SqlServer=Server=myserver;Database=TailorTalent;...
set AiService__BaseUrl=http://ai-host:8000
```

### SQL Server setup

Run [`database/TailorTalent_SqlServer.sql`](database/TailorTalent_SqlServer.sql):

```
sqlcmd -S localhost -E -i database/TailorTalent_SqlServer.sql
```

It creates the `TailorTalent` database, a `tailortalent_app` login (**change the placeholder password**), all seven tables with indexes and foreign keys, and CRUD stored procedures for every entity (`usp_<Entity>_Create/GetById/GetAllByUser/Update/Delete`, plus atomic `usp_UserCredits_Deduct` and `usp_UserSubscriptions_Upsert`). Then set `Database:Provider` to `SqlServer`.

### Frontend (`src/TailorTalent.Web/src/environments/`)

- `environment.ts` (dev): `apiUrl: 'http://localhost:5111/api'`
- `environment.prod.ts`: `apiUrl: '/api'` — expects the API behind the same origin / reverse proxy.

The frontend only talks to the backend API; it never calls the AI service directly.

### AI service (`ai-service/.env`)

Copy `.env.example` to `.env`:

```
OPENAI_API_KEY=   # empty → mock mode (deterministic sample responses, no external calls)
AI_MODEL=gpt-4o
AI_SERVICE_HOST=0.0.0.0
AI_SERVICE_PORT=8000
```

## Running locally

```bash
# AI service
cd ai-service
python -m venv .venv && .venv\Scripts\pip install -r requirements.txt
.venv\Scripts\python -m uvicorn main:app --port 8000

# Backend API
cd src/TailorTalent.Api
dotnet run            # http://localhost:5111 (Swagger UI at /)

# Frontend
cd src/TailorTalent.Web
npm install && npm start   # http://localhost:4200
```

## Tests

```bash
dotnet test tests/TailorTalent.Api.Tests          # .NET unit + integration tests
cd ai-service && .venv\Scripts\python -m pytest   # AI service tests (mock mode)
```
