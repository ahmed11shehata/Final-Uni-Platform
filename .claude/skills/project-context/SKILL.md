---
name: project-context
description: AYA_UIS university platform full project context. Use this skill whenever working on ANY part of this project — backend C# ASP.NET Core, frontend React/Vite, registration logic, academic setup, manage users page, or any new feature. Always load this skill before making any changes to avoid breaking verified working flows.
---

# AYA_UIS Project Context

## Project Identity
- **Name**: AYA_UIS / AYA_UIS_Server
- **Type**: University Information & Student Management Platform
- **Backend**: C# + ASP.NET Core Web API, Clean Architecture
- **Frontend**: React + Vite, Framer Motion, CSS Modules
- **Root path**: `D:\kak\Private\final_project\AYA_UIS_Server`

## Solution Structure
```
AYA_UIS.API                           ← Entry point, Program.cs
AYA_UIS.Application                   ← Commands, Queries, Handlers
AYA_UIS.Core/Domain                   ← Entities, Enums
AYA_UIS.Core/Services/Implementatios  ← AdminService.cs, StudentRegistrationService.cs
AYA_UIS.Infrastructure/Presentation   ← Controllers (AdminController.cs, etc.)
AYA_UIS.Infrastructure/Presistence    ← DbContext, Repositories, UnitOfWork
Shared/Dtos                           ← All DTOs
AYA_UIS.Frontend/src                  ← React frontend
```

## Frontend Structure
```
src/
  pages/admin/
    ManageUsers.jsx            ← Manage Users page (RECENTLY UPDATED)
    ManageUsers.module.css     ← CSS module (RECENTLY UPDATED)
    RegistrationManagerPage.jsx
  pages/student/
    CourseRegistrationPage.jsx
  services/api/
    adminApi.js                ← Admin API calls (RECENTLY UPDATED)
    studentApi.js
    axiosInstance.js           ← JWT interceptor, base URL
  context/
    RegistrationContext.jsx
```

---

## CRITICAL RULES — NEVER BREAK THESE

1. **Never redesign existing pages** — preserve colors, fonts, layout
2. **Never touch already-verified endpoints** unless fixing a confirmed bug
3. **Minimum file changes only** — touch only what the task requires
4. **Never restart from scratch** — always continue from current state
5. **Grade display format**: always `A-` not `A_Minus` in frontend
6. **Dates must be YYYY-MM-DD** for all registration endpoints
7. **studentId in API calls = GUID** (from `student.student.id`), not academicCode

---

## Design System (Frontend)

### CSS Variables (already defined globally)
```
--page-bg        background of pages
--card-bg        background of cards
--card-border    card border color
--border         general border color
--text-primary   main text
--text-secondary secondary text
--text-muted     muted/hint text
--accent         #818cf8  (indigo — primary accent)
--hover-bg       hover state background
```

### Year Colors
```
Year 1: #818cf8
Year 2: #22c55e
Year 3: #f59e0b
Year 4: #ef4444
```

### Standing Colors
```
excellent  → #22c55e
vgood      → #818cf8
good       → #3b82f6
pass       → #f59e0b
warning    → #f97316
probation  → #ef4444
```

### Font & Animation
- **Font**: `'Sora', sans-serif` everywhere
- **Animations**: framer-motion only (`motion`, `AnimatePresence`)
- **No inline animation on inputs/checkboxes** (causes lag)

---

## Business Logic

### Grade Mapping (numeric total → letter → GPA points)
```
97-100 → A+  → 4.0
93-96  → A   → 4.0
90-92  → A-  → 3.7
87-89  → B+  → 3.3
83-86  → B   → 3.0
80-82  → B-  → 2.7
77-79  → C+  → 2.3
73-76  → C   → 2.0
70-72  → C-  → 1.7
67-69  → D+  → 1.3
60-66  → D   → 1.0
0-59   → F   → 0.0
```

### Standing Rules (GPA → standing → maxCredits)
```
>= 3.5 → excellent  → 21 credits
>= 3.0 → vgood      → 18 credits
>= 2.5 → good       → 18 credits
>= 2.0 → pass       → 15 credits
>= 1.5 → warning    → 12 credits
>= 0.0 → probation  → 9 credits
```

### Academic Setup Rules
- Admin inputs numeric total only (60–100)
- Backend derives grade + gpaPoints
- Failed transfer courses are NOT stored
- Equivalency courses stored with isEquivalency = true
- After save: GPA, totalCreditsEarned, standing, currentYear all recalculate
- Equivalency courses count as passed prerequisites
- Equivalency courses do NOT count as active registrations
- Equivalency courses do NOT inflate currentCredits

### Registration Rules
- Student only sees courses opened for their year bucket
- Closed registration → empty list + "Registration is closed" message
- Closed registration → register/drop return 403
- Duplicate register → 409 conflict
- Credit limit exceeded → 422

---

## Verified Endpoints (DO NOT BREAK)

### Admin — Registration
```
GET  /api/admin/registration/status
POST /api/admin/registration/start
POST /api/admin/registration/stop
PUT  /api/admin/registration/settings
GET  /api/admin/courses
```

### Admin — Student Control
```
GET    /api/admin/student/{studentId}
POST   /api/admin/student/{studentId}/courses/add    body: { courseCode }
DELETE /api/admin/student/{studentId}/courses/{code}
PUT    /api/admin/student/{studentId}/lock/{code}?reason=
PUT    /api/admin/student/{studentId}/unlock/{code}
PUT    /api/admin/student/{studentId}/max-credits    body: { maxCredits }
GET    /api/admin/student/{studentId}/academic-setup
PUT    /api/admin/student/{studentId}/academic-setup
```

### Student — Registration
```
GET    /api/student/registration/status
GET    /api/student/registration/courses
GET    /api/student/courses/available
POST   /api/student/registration/courses    body: { courseCode }
DELETE /api/student/registration/courses/{courseCode}
```

---

## Key JSON Shapes

### PUT Academic Setup Request
```json
{
  "currentYear": 2,
  "years": {
    "1": { "completedCourses": [{ "courseCode": "CS101", "total": 83 }] },
    "2": { "completedCourses": [] },
    "3": { "completedCourses": [] },
    "4": { "completedCourses": [] }
  }
}
```

### GET /api/admin/student/{id} Response
```json
{
  "success": true,
  "data": {
    "student": { "id": "GUID", "name": "string", "email": "string",
                 "academicCode": "string", "year": "Second Year", "gpa": 3.35, "active": true },
    "standing": { "standingId": "vgood", "gpa": 3.35, "maxCredits": 18,
                  "mustRetakeFirst": false, "canOnlyRetake": false },
    "registeredCourses": [{ "code": "", "name": "", "credits": 3, "status": "", "grade": "" }],
    "completedCourses":  [{ "code": "", "name": "", "credits": 3, "status": "", "grade": "" }]
  }
}
```

### Standard Error Shape
```json
{ "success": false, "error": { "code": "STRING_CODE", "message": "Human readable" } }
```

---

## Completed Features
- Auth (login/logout/JWT)
- Admin Email Manager (create/toggle/reset/delete)
- Admin Registration Manager (open/close/settings/per-year courses)
- Student Course Registration (register/drop/list/status)
- Academic Setup backend (GET + PUT, GPA recalc, equivalency logic)
- ManageUsers frontend (search, profile, courses panel, grades panel, academic setup panel)
- adminApi.js (all student control + academic setup functions)

## Still Pending
- Admin student control endpoints — not fully tested in Swagger yet
- Transcript endpoint — unclear which route to use
- Send-as-email after account creation
- First-login must change password
- Grade format fix: A_Minus → A- in GET /api/admin/student/{id}

---

## Token-Saving Rules
- Read only files relevant to the current task
- Do not re-read files already shown in context
- Do not explain — just do it
- Build only when asked
- Change minimum files needed
- Do not add features not explicitly requested
