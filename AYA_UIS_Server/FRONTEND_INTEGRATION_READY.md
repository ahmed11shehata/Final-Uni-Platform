# Frontend Integration - Getting Started

## Summary

Your backend API is **100% ready** with Swagger documentation at:
```
http://localhost:7121/swagger
```

Now let's add a frontend!

---

## Choose Your Path

### 🟢 Path 1: I'll Create Everything (Recommended)

1. **You run this** (takes 2 minutes):
```powershell
npm create vite@latest AYA_UIS.Frontend -- --template react-ts
cd AYA_UIS.Frontend
npm install axios react-router-dom zustand
```

2. **Create `.env.local`**:
```env
VITE_API_URL=http://localhost:5282/api
```

3. **Tell me you're done**

4. **I'll create**:
   - ✅ Login page
   - ✅ Student dashboard
   - ✅ Course details
   - ✅ API integration
   - ✅ Navigation
   - ✅ Everything else!

### 🟡 Path 2: I Create from Scratch

Just ask me to create a complete React frontend in a specified folder, and I'll set it up for you.

---

## Why This Works

| Component | Status | Used By |
|-----------|--------|---------|
| Backend API | ✅ Complete | Frontend |
| Swagger Docs | ✅ Complete | Frontend Team |
| DTOs/Types | ✅ Complete | Frontend Types |
| Authentication | ✅ Configured | Frontend Auth |
| CORS | ✅ Ready | Frontend Requests |

---

## Simple Example

Once your frontend is ready, you'll have pages like:

### Login Page
```
┌─────────────────────────┐
│   AYA University UIS    │
│                         │
│  Email:    [ input ]    │
│  Password: [ input ]    │
│           [Login]       │
└─────────────────────────┘
```

### Student Dashboard
```
┌────────────────────────────────┐
│ Welcome, Ahmed!        [Logout] │
├────────────────────────────────┤
│ 📊 GPA: 3.5                    │
│ 📚 Courses: 5                  │
│ 📝 Assignments: 2 Pending      │
├────────────────────────────────┤
│ Your Courses:                  │
│ ┌──────────┐ ┌──────────┐      │
│ │ CS 201   │ │ MATH 301 │      │
│ │ Web Dev  │ │ Calc 3   │      │
│ └──────────┘ └──────────┘      │
└────────────────────────────────┘
```

---

## What You Get

```
AYA_UIS.Frontend/
├── src/
│   ├── api/
│   │   └── client.ts              ← Connects to your backend
│   ├── pages/
│   │   ├── LoginPage.tsx
│   │   ├── StudentDashboard.tsx
│   │   ├── CoursesPage.tsx
│   │   └── ...
│   ├── components/
│   │   ├── Navbar.tsx
│   │   ├── Sidebar.tsx
│   │   ├── CourseCard.tsx
│   │   └── ...
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   ├── useStudent.ts
│   │   └── ...
│   ├── types/
│   │   └── index.ts              ← From your backend DTOs
│   └── App.tsx
├── .env.local                     ← API URL
└── package.json
```

---

## Integration Architecture

```
┌──────────────────────────┐
│   React Frontend         │
│  (Port 5173)             │
└──────────────┬───────────┘
               │
        axios with JWT
               │
               ↓
┌──────────────────────────┐
│   ASP.NET 8 Backend      │
│  (Port 7121/5282)        │
│                          │
│  - Authentication        │
│  - Student Module        │
│  - Instructor Module     │
│  - Admin Module          │
└──────────────────────────┘
               │
               ↓
┌──────────────────────────┐
│   SQL Server Database    │
└──────────────────────────┘
```

---

## How Communication Works

### 1. User Logs In
```
Frontend:                          Backend:
[Email, Password] ────────POST────→ /api/auth/login
                                  ↓
                        [Validate Credentials]
                                  ↓
[Token, User Info] ←────200 OK────[JWT Token]

Frontend saves token in localStorage
```

### 2. User Views Dashboard
```
Frontend:                          Backend:
[Token in Header] ────GET─────────→ /api/student/dashboard
Authorization: Bearer [token]
                                  ↓
                    [Validate JWT, Get User Data]
                                  ↓
[Dashboard Data] ←────200 OK────[JSON Response]
(courses, grades, etc)
```

### 3. User Submits Assignment
```
Frontend:                          Backend:
[File + FormData] ───POST────────→ /api/student/assignments/123/submit
[Auth Header]                    ↓
                    [Validate JWT, Save File]
                                  ↓
[Success Message] ←────201────────[Submission ID]
```

---

## Getting Started Now

### Option A: Let Me Create It (Easiest)

Just tell me:
> "Create a React frontend project at D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend"

And I'll:
1. ✅ Create project structure
2. ✅ Set up API client
3. ✅ Create login page
4. ✅ Create dashboard
5. ✅ Create all components
6. ✅ Connect to your backend

### Option B: You Create It (5 min)

```powershell
# Run this one command:
npm create vite@latest AYA_UIS.Frontend -- --template react-ts

# Install dependencies:
cd AYA_UIS.Frontend
npm install

# Then tell me you're done!
```

---

## What's Ready on Backend

✅ **60+ API Endpoints**
✅ **JWT Authentication**
✅ **Swagger Documentation**
✅ **CORS Configured**
✅ **Database Seeding**
✅ **Error Handling**
✅ **Rate Limiting**
✅ **Request Validation**

---

## Next Action

Choose one:

**A)** Run: `npm create vite@latest AYA_UIS.Frontend -- --template react-ts` → Tell me when done

**B)** Just tell me: "Create React frontend for me"

**C)** Ask me: "Help me set up [framework name]"

---

**I'm ready to build your complete frontend!** 🚀

What would you like to do?
