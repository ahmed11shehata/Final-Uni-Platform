# AYA University Information System

A full-stack university management platform built with ASP.NET Core 8 and React 19. Supports three distinct user roles — **Admin**, **Instructor**, and **Student** — each with a dedicated portal, live registration control, academic record management, course scheduling, and an integrated AI study assistant.

> Copyright (c) Ahmed Mohamed Abdel Fattah — see [NOTICE](NOTICE)

---

## Features

### Admin Portal
- **Email Manager** — create, activate/deactivate, reset, and delete user accounts
- **Registration Manager** — open/close registration windows, set deadlines, configure per-year course offerings
- **Student Control** — force-add/drop courses, lock/unlock registrations, override credit limits, manage academic setup (historical grades, GPA recalculation, equivalency courses)
- **Schedule Manager** — create weekly lecture sessions and exam entries, publish schedules per year/group
- **Instructor Control** — manage instructor course assignments
- **Dashboard** — platform-wide statistics

### Student Portal
- **Course Registration** — register and drop courses during open windows, with credit-limit enforcement and prerequisite checks
- **Grades & Transcript** — per-semester grade breakdown, GPA tracking, academic standing
- **Timetable & Schedule** — published weekly schedule and exam timetable
- **Course Detail** — lectures, assignments, quizzes, midterm info per enrolled course
- **AI Study Tools** — document summarization, quiz generation, mind maps, question banks, chat assistant (powered by a separate FastAPI + Mistral AI server)
- **Theming** — 6 visual atmosphere themes (Ramadan, Space, Arctic, Pharaoh, Saladin, Fog)
- **Profile** — view and update personal information

### Instructor Portal
- **Dashboard** — enrolled students, grade summaries, recent activity
- **Quiz Builder** — create quizzes with multiple-choice questions
- **Assignment Management** — create assignments and grade submissions
- **Lecture Upload** — upload course materials
- **Grade Management** — review and approve student results

### Authentication
- JWT Bearer authentication with RSA asymmetric key signing
- Role-based access control (Admin / Student / Instructor)
- Forgot password via OTP sent to a recovery sub-email (Brevo email provider)
- Server-side token revocation on logout
- Change password flow with `MustChangePassword` flag

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend runtime | .NET 8 / ASP.NET Core 8 |
| Architecture | Clean Architecture + CQRS (MediatR) |
| ORM | Entity Framework Core 8 (Code-First, 19 migrations) |
| Database | SQL Server |
| Identity | ASP.NET Core Identity + JWT |
| Email | Brevo API (SMTP fallback) |
| Frontend runtime | React 19 + Vite 8 |
| Routing | React Router DOM 7 |
| HTTP client | Axios with JWT interceptor |
| Animation | Framer Motion 12 + GSAP 3 |
| Notifications | react-hot-toast |
| Styling | CSS Modules, CSS custom properties |
| AI server | FastAPI (Python) + Mistral AI |

---

## Project Structure

```
final_project/
├── AYA_UIS_Server/                    # Main .NET + React monorepo
│   ├── AYA_UIS.API/                   # Entry point — Program.cs, middleware, config
│   ├── AYA_UIS.Application/           # CQRS commands, queries, handlers, AutoMapper profiles
│   ├── AYA_UIS.Core/
│   │   ├── Abstractions/              # Service and repository interfaces
│   │   ├── Domain/                    # Entities, enums, repository contracts
│   │   └── Services/                  # Service implementations
│   ├── AYA_UIS.Infrastructure/
│   │   ├── Presentation/              # Controllers (26 endpoints groups)
│   │   └── Presistence/               # DbContext, EF migrations, repositories, UnitOfWork
│   ├── Shared/                        # DTOs, exceptions, common helpers
│   ├── AYA_UIS.Frontend/              # React frontend (Vite)
│   │   └── src/
│   │       ├── pages/admin/           # 8 admin pages
│   │       ├── pages/student/         # 14 student pages + 6 AI tool pages
│   │       ├── pages/instructor/      # 6 instructor pages
│   │       ├── pages/auth/            # Login, forgot password, change password
│   │       ├── context/               # Auth, Theme, Notification, Registration, Schedule
│   │       ├── services/api/          # Axios modules per role
│   │       └── components/            # Layout, sidebar, navbar, atmosphere overlays
│   └── *.UnitTests/                   # xUnit test projects (Application, Domain, Presentation, Shared)
├── ai-study-assistant-api-main/       # FastAPI AI server (Python)
├── docs/
│   ├── architecture.md                # Technical architecture reference
│   └── setup.md                       # Installation and run guide
├── README.md
└── NOTICE
```

---

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQL Server (local instance)
- Python 3.11+ (optional — only needed for AI tools)

### 1 — Backend
```bash
cd AYA_UIS_Server/AYA_UIS.API
dotnet run
```
API: `https://localhost:7121`  
Swagger UI: `https://localhost:7121/swagger`

### 2 — Frontend
```bash
cd AYA_UIS_Server/AYA_UIS.Frontend
npm install
npm run dev
```
App: `http://localhost:5173`

### 3 — AI Server (optional)
```bash
cd ai-study-assistant-api-main
pip install -r requirements.txt
python fastapi_app.py
```

See **[docs/setup.md](docs/setup.md)** for full setup including database migration, environment configuration, and secrets.

---

## Documentation

| Document | Purpose |
|---|---|
| [docs/architecture.md](docs/architecture.md) | Technical architecture, layers, data flow, module design |
| [docs/setup.md](docs/setup.md) | Full installation and configuration guide |
