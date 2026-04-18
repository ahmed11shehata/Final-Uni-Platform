# Architecture — AYA University Information System

## Overview

AYA UIS is structured as a Clean Architecture .NET 8 Web API with a React 19 SPA frontend. The backend enforces strict layer separation: domain logic never depends on infrastructure, and all cross-cutting concerns (auth, error handling, rate limiting) are handled at the API entry point.

```
┌─────────────────────────────────────────────────────────────────┐
│                        React 19 SPA                             │
│              (Vite · CSS Modules · Framer Motion)               │
└────────────────────────┬────────────────────────────────────────┘
                         │ HTTPS · JWT Bearer
┌────────────────────────▼────────────────────────────────────────┐
│                    ASP.NET Core 8 API                           │
│   ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐  │
│   │ Presentation │  │  Application │  │       Core         │  │
│   │ (Controllers)│→ │ CQRS/MediatR │→ │ Domain + Services  │  │
│   └──────────────┘  └──────────────┘  └────────────────────┘  │
│                                        ┌────────────────────┐  │
│                                        │   Persistence      │  │
│                                        │  EF Core · SQL Srv │  │
│                                        └────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│              FastAPI AI Server (Python · Mistral AI)            │
└─────────────────────────────────────────────────────────────────┘
```

---

## Backend Architecture

### Layer Responsibilities

#### `AYA_UIS.API`
Entry point for the whole backend. Owns:
- `Program.cs` — DI registration, middleware pipeline, CORS, JWT config, Swagger, rate limiter
- Global exception handling middleware
- Token blocklist middleware (runs after `UseAuthentication`, before `UseAuthorization`)
- RSA key loading for JWT signing (`Keys/*.pem`)
- Seed/factory services

CORS is configured to allow `http://localhost:5173` and `https://localhost:5173` (Vite dev server) and `http://localhost:3000`.

#### `AYA_UIS.Application`
CQRS layer using MediatR. Contains:
- **Commands** — state-mutating operations (CreateRegistration, OpenCourses, LockCourse, SubmitQuiz, GradeSubmission, etc.)
- **Queries** — read-only operations (GetRegisteredCourses, GetRegistrationStatus, GetStudyYear, etc.)
- **Handlers** — one handler per command/query, injected via MediatR pipeline
- **AutoMapper profiles** — entity-to-DTO mapping for Application-layer responses

This layer is used primarily for complex academic operations. Direct service calls are used for simpler admin operations (see `AdminService`).

#### `AYA_UIS.Core`

**Abstractions** — interfaces only:
- `IServiceManager` — aggregates all service interfaces into a single injectable facade
- `IAdminService`, `IStudentRegistrationService`, `IAuthenticationService`, `IEmailService`

**Domain** — pure C# entities and repository contracts:
- Entity models (see [Domain Entities](#domain-entities) below)
- `IUnitOfWork` and `IRepository<T>` contracts
- Enums: `Levels` (year 1–4), `Grads` (A+ through F), `Gender`

**Services** — concrete service implementations:
- `AuthenticationService` — login, register, JWT issuance, password operations
- `AdminService` — email account CRUD, registration control, schedule management, student academic control
- `StudentRegistrationService` — course availability, registration/drop, credit limit enforcement, prerequisite checking, GPA recalc
- `EmailService` — Brevo API with SMTP fallback

#### `AYA_UIS.Infrastructure/Presentation`
26 controller files grouped by domain area. All controllers return a consistent envelope:
```json
{ "success": true,  "data": { ... } }
{ "success": false, "error": { "code": "STRING_CODE", "message": "..." } }
```

Rate limiting (`PolicyLimitRate`) is applied globally to all controllers.

#### `AYA_UIS.Infrastructure/Presistence`
- `UniversityDbContext` extends `IdentityDbContext<User>` — owns all DbSets
- 19 EF Core Code-First migrations
- `UnitOfWork` implements `IUnitOfWork` — aggregates all repositories
- Generic `Repository<T>` base + specialised repositories per entity

#### `Shared`
Cross-cutting concerns independent of any layer:
- All DTOs grouped by module (`Auth_Module`, `Admin_Module`, `Student_Module`, `Info_Module`, etc.)
- Base exception types (`BaseException`, `UnauthorizedException`, `NotFoundException`, etc.)
- Common helpers

---

### Domain Entities

| Entity | Description |
|---|---|
| `User` | ASP.NET Identity user — students, instructors, admins. Fields: `Academic_Code`, `Level`, `DepartmentId`, `SubEmail`, `MustChangePassword`, `ProfilePicture` |
| `PasswordResetOtp` | OTP record with expiry, attempt counter, and used flag |
| `Course` | Course definition with code, credits, prerequisites, year level |
| `CourseOffering` | Links a course to a specific registration window/year bucket |
| `CoursePrerequisite` | Course dependency graph |
| `CourseResult` | Admin-assigned grade record: `Total` (60–100), `Grade` (letter), `GpaPoints`, `IsEquivalency`, `IsPassed` |
| `Registration` | Student's active course registration row. Has `IsEquivalency` flag and `Grade` |
| `RegistrationSettings` | Single-row table controlling the registration window: `IsOpen`, `Deadline`, `Semester`, per-year credit limits |
| `RegistrationCourseInstructor` | Maps courses opened for registration to specific instructors |
| `StudyYear` / `UserStudyYear` | Academic year setup for a student |
| `Semester` / `SemesterGPA` | GPA per semester for transcript |
| `ScheduleSession` | Weekly lecture slot: day, start/end time, year, group, room, instructor |
| `ExamScheduleEntry` | Exam record: date, time, hall, course, type (midterm/final) |
| `SchedulePublish` | Marks when a schedule was last published |
| `AcademicSchedule` | Aggregate schedule record |
| `Assignment` / `AssignmentSubmission` | Assignment lifecycle |
| `Quiz` / `QuizQuestion` / `QuizOption` / `StudentQuizAttempt` / `StudentAnswer` | Quiz engine |
| `AdminCourseLock` | Admin-placed lock on a student's course with optional reason |
| `StudentCourseException` | Admin override granting a student access to a normally ineligible course |
| `Department` | Institutional department |
| `Fee` | Financial records |

---

### Authentication & Authorization Flow

```
1. POST /api/authentication/login
   → AuthenticationService validates credentials via UserManager
   → Issues RSA-signed JWT (1-day expiry by default)
   → Returns { success, data: { token, user: { id, name, role, ... } } }

2. Frontend stores token in localStorage
   → Axios interceptor attaches: Authorization: Bearer <token>

3. All protected routes require [Authorize] attribute
   → Role-based: [Authorize(Roles = "Admin")] / "Student" / "Instructor"

4. POST /api/authentication/logout
   → Token added to server-side blocklist
   → TokenBlocklistMiddleware checks every request against blocklist

5. Forgot password flow:
   POST /api/authentication/forgot-password/request
   → Sends 6-digit OTP to user.SubEmail via Brevo
   → OTP valid 10 minutes, max 3 attempts
   POST /api/authentication/forgot-password/verify
   → Validates OTP, resets password
```

---

### Registration Flow

The registration system has two controllers serving different purposes:

**`RegistrationSettingsController`** (`/api/registration-settings`) — CQRS-based, handles the registration window state.

**`StudentController`** (`/api/student`) — direct service, handles per-student operations during the window.

**`AdminController`** (`/api/admin/registration/*`) — admin management of the window.

```
Admin opens window:
  POST /api/admin/registration/start
    → AdminService.StartRegistrationAsync
    → Creates/updates RegistrationSettings row: IsOpen=true, Deadline, per-year seats/credit-limits
    → Opens CourseOfferings for specified year buckets

Student checks status:
  GET /api/student/registration/status
    → Returns: { isOpen, deadline, semester, maxCredits, currentCredits }

Student fetches available courses:
  GET /api/student/registration/courses
    → Filters CourseOfferings by student's Level
    → Excludes already-registered, already-passed, locked courses
    → Returns courses with prerequisite-met flag

Student registers:
  POST /api/student/registration/courses  { courseCode }
    → Validates: window open, not duplicate, prerequisites met, credits within limit
    → 409 on duplicate, 422 on credit overflow, 403 if closed

Admin closes window:
  POST /api/admin/registration/stop
    → Sets IsOpen=false
```

---

### Academic Setup & GPA Calculation

Admin-managed historical record for a student. Used to set up transfer students or correct historical data.

```
PUT /api/admin/student/{studentId}/academic-setup
  Body: {
    currentYear: 2,
    years: {
      "1": { completedCourses: [{ courseCode, total }] },
      ...
    }
  }

Processing:
  1. For each course: total (60–100) → letter grade + GPA points (backend-derived)
  2. Failed courses (total < 60) NOT stored
  3. Equivalency courses: isEquivalency=true, count as passed prereqs, excluded from active credit count
  4. After save: recalculate cumulative GPA, totalCreditsEarned, standing, currentYear

Standing table:
  GPA ≥ 3.5 → excellent → 21 max credits
  GPA ≥ 3.0 → vgood     → 18 max credits
  GPA ≥ 2.5 → good      → 18 max credits
  GPA ≥ 2.0 → pass      → 15 max credits
  GPA ≥ 1.5 → warning   → 12 max credits
  GPA ≥ 0.0 → probation →  9 max credits
```

---

## Frontend Architecture

### Structure

```
src/
├── App.jsx                    # Root — providers + React Router route tree
├── main.jsx                   # Entry point
├── context/
│   ├── AuthContext.jsx        # JWT storage, login/logout, current user
│   ├── ThemeContext.jsx       # Active atmosphere theme (6 options)
│   ├── NotificationContext.jsx
│   ├── RegistrationContext.jsx # Registration window state (shared across student pages)
│   └── ScheduleContext.jsx
├── components/
│   ├── common/
│   │   ├── Sidebar.jsx        # Role-aware sidebar navigation
│   │   ├── Navbar.jsx
│   │   ├── ProtectedRoute.jsx # Role-based route guard
│   │   └── *Atmosphere.jsx    # 6 visual theme overlays (Ramadan, Space, Arctic, Pharaoh, Saladin, Fog)
│   └── layout/
│       ├── MainLayout.jsx     # Sidebar + content wrapper
│       └── Topbar.jsx
├── pages/
│   ├── auth/                  # Login, ForgotPassword, ChangePassword, PasswordHelp
│   ├── admin/                 # 8 pages
│   ├── instructor/            # 6 pages
│   ├── student/               # 14 pages + tools/
│   │   └── tools/             # 6 AI tool pages
│   └── shared/                # Settings (shared by all roles)
├── services/
│   ├── api/
│   │   ├── axiosInstance.js   # Base URL, JWT interceptor, global error handling
│   │   ├── authApi.js
│   │   ├── adminApi.js        # Email, registration, student control, academic setup
│   │   ├── studentApi.js      # Registration, courses, grades, schedule
│   │   ├── instructorApi.js
│   │   └── scheduleApi.js
│   ├── aiApi.js               # Calls to FastAPI AI server
│   └── mock/mockData.js       # Static fallback data
└── hooks/
    └── useAuth.js
```

### Route Protection

All role areas are wrapped in `ProtectedRoute` which checks the role stored in `AuthContext`. Unauthorized role access redirects to `/login`.

### API Layer

`axiosInstance.js` centralises all HTTP behaviour:
- Base URL: `VITE_API_BASE_URL` env var or `https://localhost:7121/api`
- 30-second timeout
- Auto-attach JWT from `localStorage`
- 401 on non-login pages → clear storage → redirect to `/login`
- 429 → user-visible rate limit message

### Design System

All pages share a global CSS variable set:

```css
--page-bg, --card-bg, --card-border, --border
--text-primary, --text-secondary, --text-muted
--accent: #818cf8  (indigo)
--hover-bg
```

Font: `Sora` (Google Fonts). Animations: Framer Motion only (`motion`, `AnimatePresence`). No inline animation on inputs/checkboxes (performance).

Year accent colors: `#818cf8` · `#22c55e` · `#f59e0b` · `#ef4444` (years 1–4).

---

## AI Study Assistant (Separate Server)

Located in `ai-study-assistant-api-main/`. FastAPI Python server.

- **`fastapi_app.py`** — main API, 10 endpoints
- **`utils.py`** — document processing utilities
- **`config.py`** — Mistral AI and environment config

Capabilities:
- Parse PDF, Word, PowerPoint, and image files
- Generate summaries, quizzes, mind maps, question banks
- Document-grounded chat (Mistral AI)
- Bilingual (Arabic + English)

The React frontend calls this server through `services/aiApi.js`. The AI server runs independently from the .NET backend.

---

## Unit Tests

Four xUnit test projects:

| Project | Tests cover |
|---|---|
| `AYA_UIS.Application.UnitTests` | CQRS command/query handlers |
| `Domain.UnitTests` | Domain entity logic |
| `Presentation.UnitTests` | Controller auth guards, response shapes |
| `Shared.UnitTests` | Exception types, shared utilities |
