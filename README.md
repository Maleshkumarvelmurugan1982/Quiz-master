# 🧠 QuizMaster — ASP.NET Web Forms (.aspx)

**C# · ASP.NET Web Forms (.NET Framework 4.8) · SQL Server Express · ADO.NET**

This is the QuizMaster project rebuilt with the **ASP.NET Web Forms (ASPX) framework**:
real `.aspx` pages with code-behind (`.aspx.cs`), a Master Page, a User Control (`.ascx`),
server controls (GridView, Repeater, Button, TextBox, DropDownList, FileUpload…), postbacks,
ViewState, Session and Forms Authentication.

---

## ▶️ How to run (2 minutes)

**You need:** Visual Studio 2019 or 2022 with the **"ASP.NET and web development"** workload,
and **SQL Server Express** (the same one the old project used).

1. Extract the zip.
2. Double-click **`QuizMaster.sln`** (opens in Visual Studio).
3. Press **F5** (or the green ▶ IIS Express button).

That's it. On the first run the app **creates the `QuizAppDB` database automatically**
(from `QuizApp/App_Data/schema.sql`) if it does not exist yet. If you already have `QuizAppDB`
from the old project, it is used as-is — same tables, same users.

### Login

| Role  | Email               | Password  |
|-------|---------------------|-----------|
| Admin | admin@quizapp.com   | Admin@123 |
| User  | malesh@quizapp.com  | Admin@123 |

Or click **Register** to create a new student account.

### If the login page says "Cannot connect to SQL Server"

Open `QuizApp/Web.config` and change `Data Source` in the `DefaultConnection` connection string:

| Your setup                         | Use                                   |
|-----------------------------------|----------------------------------------|
| SQL Server Express (default)       | `Data Source=.\SQLEXPRESS`             |
| LocalDB (comes with Visual Studio) | `Data Source=(localdb)\MSSQLLocalDB`   |
| Full SQL Server / Developer        | `Data Source=.` or `Data Source=localhost` |

Then press F5 again. (You can also create the DB manually by running `Database/schema.sql` in SSMS.)

---

## ✅ Features

**Students**
- Register / Login (BCrypt passwords, Forms Authentication cookie) — students pick their **Section (A, B or C)**
- Dashboard with stats + performance bar chart
- Browse quizzes — search, filter by category (incl. sub-categories) and difficulty
- Take a quiz: one question at a time, question navigator, progress bar, **countdown timer**,
  auto-submit when time runs out, bookmark questions while answering
- Timer is kept on the **server**, so refreshing the page does not reset it; answers survive a refresh
- Result page: score ring, pass/fail, correct/wrong/unanswered, full answer review with explanations
- Attempt history, Leaderboard, Bookmarks
- Dark mode toggle (remembered)

**Admin**
- Admin dashboard (users, quizzes, questions, attempts, quiz performance, recent activity)
- ✨ **Generate Quiz with AI** — from a topic or an uploaded file
  (PDF, DOCX, PPTX, TXT, MD, CSV, JSON, images; max 10 MB) → review/edit → save
- Create / edit quizzes with their questions on one page (add/remove questions)
- Manage questions (add, edit, hide/show, delete)
- Quiz options: duration, pass %, negative marking, randomize questions, **shuffle answer options**,
  max questions per attempt, active/inactive
- **Students by section:** cards showing how many students are in Section A / B / C (click to filter),
  a section filter + search, and a dropdown in each row to move a student to another section
- Manage users (enable/disable, view a user's attempts and results)

---

## 📁 Project structure

```
QuizMaster.sln
QuizApp/
├── Site.Master (+ .cs)          ← Master page: sidebar, menu, theme, toasts
├── Login.aspx, Register.aspx    ← Public pages
├── Default.aspx                 ← Sends user to the right dashboard
├── Dashboard.aspx               ← Student dashboard
├── Quizzes.aspx                 ← Browse/search quizzes
├── QuizDetail.aspx              ← Quiz info + Start button
├── TakeQuiz.aspx                ← Taking a quiz (timer, navigator, bookmarks)
├── Result.aspx                  ← Score + answer review
├── History.aspx                 ← GridView of past attempts
├── Leaderboard.aspx, Bookmarks.aspx
├── Admin/                       ← Admin-only folder (protected by Admin/Web.config)
│   ├── Dashboard.aspx
│   ├── Quizzes.aspx             ← GridView with Edit / Activate / Delete
│   ├── QuizEdit.aspx            ← Create/edit quiz + questions
│   ├── Questions.aspx, QuestionEdit.aspx
│   ├── GenerateQuiz.aspx        ← AI quiz generator (2 steps)
│   ├── Users.aspx, UserAttempts.aspx
│   └── Web.config               ← <allow roles="Admin" />
├── Controls/QuestionEditor.ascx ← Reusable user control: editable list of questions
├── Handlers/Bookmark.ashx       ← Small AJAX handler for bookmarking during a quiz
├── Code/                        ← Business logic + data access (C# classes)
│   ├── Db.cs                    ← ADO.NET helper (parameterized SQL)
│   ├── DbInitializer.cs         ← Creates QuizAppDB on first run
│   ├── Auth.cs, AuthService.cs  ← Forms Authentication + login/register
│   ├── BCrypt.cs                ← BCrypt password hashing (works with existing hashes)
│   ├── QuizService.cs, QuestionService.cs, AttemptService.cs,
│   │   CategoryService.cs, UserService.cs
│   ├── QuizGeneratorService.cs  ← AI generation via OpenRouter
│   ├── DocumentTextExtractor.cs ← Reads DOCX / PPTX / text files
│   ├── Models.cs, BasePage.cs, Fmt.cs, Parse.cs, ReviewHtml.cs
├── Content/site.css, Scripts/site.js, Scripts/takequiz.js
├── App_Data/schema.sql          ← Used to auto-create the database
├── Database/                    ← schema.sql, migrate_existing_db.sql, add_sections.sql
├── Global.asax                  ← App start (DB check) + login cookie → user/role
└── Web.config                   ← Connection string, AI key, auth rules
```

No NuGet packages are needed — everything uses the .NET Framework itself, so the project builds offline.

---

## 🔄 What changed from the old version

| Old (ASP.NET Core 8 + HTML)                  | New (ASP.NET Web Forms .aspx)                          |
|---------------------------------------------|--------------------------------------------------------|
| One `wwwroot/index.html` + JavaScript `fetch` | Separate `.aspx` pages rendered on the server          |
| API controllers (`/api/quiz`, …)             | Code-behind event handlers (`Button_Click`, `RowCommand`) |
| JWT token in localStorage                    | Forms Authentication cookie + `<authorization>` rules  |
| Entity Framework Core                        | ADO.NET (`SqlConnection`, parameters, transactions)    |
| `appsettings.json`                            | `Web.config`                                           |
| BCrypt.Net NuGet package                     | Built-in `Code/BCrypt.cs` (same hash format)           |
| DocumentFormat.OpenXml package               | `ZipArchive` + `XDocument` (built into .NET)           |

The database schema is the same, plus one new column `Users.Section`. If you already have
`QuizAppDB` from the old project, the app **adds that column automatically** when it starts
(or run `Database/add_sections.sql`). Old students show as "No section" until the admin assigns one.

**Fixes made along the way**
- "Shuffle answer options" now grades correctly (before, shuffled letters were graded against the original letter).
- "Max questions per attempt" now scores out of the questions actually shown.
- Admins can see and re-show hidden questions; deleting a quiz that has attempts no longer fails.
- If OpenRouter rejects a request (bad key / model), the error is shown at once instead of after a 3-minute wait.

---

## 🤖 AI quiz generation (OpenRouter)

Settings are in `Web.config` → `<appSettings>`:

```xml
<add key="OpenRouter:ApiKey" value="sk-or-v1-..." />
<add key="OpenRouter:Model"  value="openrouter/free" />
```

Your existing key from the old project is already there. If it stops working,
get a free key at https://openrouter.ai/keys and paste it in. Free models can be busy;
temporary errors are retried automatically.

---

## 💡 Tech stack

| Layer     | Technology                                   |
|-----------|----------------------------------------------|
| Framework | ASP.NET Web Forms, .NET Framework 4.8        |
| Language  | C#                                           |
| Database  | SQL Server Express (ADO.NET / SqlClient)     |
| Auth      | Forms Authentication + role-based folders    |
| Passwords | BCrypt                                       |
| AI        | OpenRouter (OpenAI-compatible API)           |
| UI        | Server controls + CSS (Inter / JetBrains Mono) |
