# 🧠 QuizMaster — Full Stack Quiz Application
### C# ASP.NET Core 8 + SQL Server Express + HTML/CSS/JS Frontend

---

## ✅ FEATURES
- User Register / Login (JWT Auth)
- Browse Quizzes (Search + Filter by Category/Difficulty)
- Take Quiz with Countdown Timer
- Auto-submit when timer ends
- Result Page: Score, %, Correct/Wrong/Unanswered
- Full Answer Review with Explanations
- Attempt History
- Leaderboard
- Bookmark Questions
- User Dashboard with Performance Chart
- **Admin Panel:**
  - ✨ **Generate Quiz with AI** — from a topic OR an uploaded file (PDF, DOCX, PPTX, TXT, MD, CSV, JSON, images; max 10 MB)
  - Review / edit / remove generated questions before saving
  - Create/Edit/Delete Quizzes (manual)
  - Add/Edit/Delete Questions
  - Manage Users (activate/deactivate)
  - Admin Dashboard with stats
- Dark Mode Toggle

---

## 🚀 SETUP INSTRUCTIONS

### STEP 1 — Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server Express (free): https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- SQL Server Management Studio (SSMS) — optional but recommended

### STEP 2 — Setup Database
1. Open SSMS or run via sqlcmd
2. Open the file: `Database/schema.sql`
3. Run the entire script
4. This creates the database, all tables, and seed data

### STEP 3 — Configure Connection String
Open `appsettings.json` and update if needed:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=QuizAppDB;Trusted_Connection=True;TrustServerCertificate=True;"
```
If your SQL Server instance name is different, change `SQLEXPRESS` to your instance name.

### STEP 3b — Add your Gemini API key (needed for AI quiz generation)
Get a free key at https://aistudio.google.com/apikey, then store it as a user-secret (keeps it out of source control):
```bash
cd QuizApp
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "AIza..."
```
(Or put it in `appsettings.json` under `Gemini:ApiKey` for quick local testing only.)

**Automatic model fallback:** the app asks Google which models your key can use (`models.list`), ranks them
(flash → pro → flash-lite, newest first) and tries them one by one. If a model fails (quota / rate limit,
overloaded, unsupported input, bad output) the next one is used automatically. Models that just failed are
skipped for a few minutes. In the generator dialog the admin can also pick a specific model to try first.

Optional settings in `appsettings.json`:
```json
"Gemini": { "ApiKey": "", "PreferredModels": ["gemini-2.5-flash"], "MaxModelAttempts": 8 }
```

> Already created the database with the old `schema.sql`? Run `Database/migrate_existing_db.sql`
> once — it removes the 3 default quizzes and fixes the seeded admin password hash.

### STEP 4 — Run the App
```bash
cd QuizApp
dotnet restore
dotnet run
```

### STEP 5 — Open in Browser
- **App:** http://localhost:5000
- **Swagger API Docs:** http://localhost:5000/swagger

---

## 🔑 LOGIN CREDENTIALS (Seeded)

| Role  | Email                  | Password   |
|-------|------------------------|------------|
| Admin | admin@quizapp.com      | Admin@123  |
| User  | malesh@quizapp.com     | Admin@123  |

---

## 📁 PROJECT STRUCTURE
```
QuizApp/
├── Controllers/
│   ├── AuthController.cs       ← Register, Login
│   ├── QuizController.cs       ← Quiz CRUD
│   ├── QuestionController.cs   ← Question CRUD + Bookmarks
│   ├── AttemptController.cs    ← Start/Submit Quiz, Results
│   ├── CategoryController.cs   ← Categories
│   └── AdminController.cs      ← Admin Dashboard
├── Models/
│   └── Models.cs               ← All Entity Classes
├── DTOs/
│   └── DTOs.cs                 ← Request/Response DTOs
├── Data/
│   └── AppDbContext.cs         ← EF Core DbContext
├── Services/
│   ├── AuthService.cs          ← Auth logic + JWT
│   ├── QuizService.cs          ← Quiz business logic
│   ├── QuizGeneratorService.cs ← AI quiz generation (Gemini, model fallback)
│   ├── GeminiModelCatalog.cs   ← loads/ranks all available Gemini models
│   ├── DocumentTextExtractor.cs← DOCX / PPTX / TXT text extraction
│   ├── QuestionService.cs      ← Question logic
│   └── AttemptService.cs       ← Quiz attempt + scoring
├── Database/
│   └── schema.sql              ← Full SQL Server schema
├── wwwroot/
│   └── index.html              ← Complete Frontend SPA
├── Program.cs                  ← App startup + DI
├── appsettings.json            ← Config
└── QuizApp.csproj              ← NuGet packages
```

---

## 📡 API ENDPOINTS

| Method | Endpoint | Access |
|--------|----------|--------|
| POST | /api/auth/register | Public |
| POST | /api/auth/login | Public |
| GET | /api/quiz | User/Admin |
| POST | /api/quiz | Admin |
| GET | /api/quiz/models | Admin (Gemini models available to your API key) |
| POST | /api/quiz/generate | Admin (multipart: topic and/or file → returns a draft, saves nothing) |
| POST | /api/quiz/with-questions | Admin (saves quiz + questions in one transaction) |
| PUT | /api/quiz/{id} | Admin |
| DELETE | /api/quiz/{id} | Admin |
| PATCH | /api/quiz/{id}/toggle | Admin |
| GET | /api/question/quiz/{quizId} | User/Admin |
| POST | /api/question | Admin |
| PUT | /api/question/{id} | Admin |
| DELETE | /api/question/{id} | Admin |
| POST | /api/question/{id}/bookmark | User |
| GET | /api/question/bookmarks | User |
| POST | /api/attempt/start/{quizId} | User |
| POST | /api/attempt/submit | User |
| GET | /api/attempt/history | User |
| GET | /api/attempt/dashboard | User |
| GET | /api/attempt/leaderboard | User/Admin |
| GET | /api/admin/dashboard | Admin |
| GET | /api/users | Admin |
| PATCH | /api/users/{id}/toggle | Admin |
| GET | /api/categories | User/Admin |

---

## 🗄️ DATABASE TABLES
- **Users** — accounts, roles, auth
- **Categories** — quiz categories with parent/sub hierarchy
- **Quizzes** — quiz settings, difficulty, timer, pass mark
- **Questions** — MCQ with 4 options, correct answer, explanation
- **Attempts** — quiz session with score, time, pass/fail
- **AttemptAnswers** — per-question answer record
- **BookmarkedQuestions** — user's saved questions

---

## 💡 TECH STACK
| Layer | Technology |
|-------|-----------|
| Backend | C# ASP.NET Core 8 |
| Database | SQL Server Express |
| ORM | Entity Framework Core 8 |
| Auth | JWT Bearer Tokens |
| Password | BCrypt hashing |
| AI | Google Gemini API (quiz generation, auto model fallback) + DocumentFormat.OpenXml |
| API Docs | Swagger / OpenAPI |
| Frontend | Vanilla HTML + CSS + JS |
| Fonts | Inter + JetBrains Mono |
