# 🎯 Frontend Integration - Visual Guide

## What You Have Now

```
Your Computer:
├── Backend Project (in VS Studio)
│   └── D:\kak\index ()\final_project\AYA_UIS_Server\
│       ✅ Working & Ready
│
└── Frontend Project (somewhere else)
    └── C:\Users\...\or D:\...\your-frontend\
        ✅ Already built
        ❌ Not in workspace yet
```

## What We're Doing

```
Step 1: MOVE
Your-Frontend (somewhere)  ─→  D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend
                                 ↓
Step 2: CONFIGURE
                              Set API URL to backend
                                 ↓
Step 3: CONNECT
                              Frontend talks to Backend
                                 ↓
✅ DONE - Both working together!
```

## Final Result

```
D:\kak\index ()\final_project\AYA_UIS_Server\

├── 🔧 AYA_UIS.API\
│   └── Backend (ASP.NET Core)
│       🚀 Runs on http://localhost:5282
│       📊 Swagger on http://localhost:7121/swagger
│
├── 🎨 AYA_UIS.Frontend\         ← YOUR PROJECT HERE
│   └── Frontend (React/Vue/Angular)
│       🚀 Runs on http://localhost:5173
│       📡 Talks to backend via API
│
├── 📦 Shared\
├── 🗂️  Domain\
└── 🗄️  Presistence\
```

## How It Works

```
User Opens Frontend
     ↓
http://localhost:5173
     ↓
[React/Vue/Angular App Loads]
     ↓
User Logs In
     ↓
Frontend sends: POST /api/auth/login
     ↓
[Request goes to Backend]
     ↓
http://localhost:5282/api/auth/login
     ↓
Backend validates & returns JWT token
     ↓
Frontend stores token
     ↓
User sees Dashboard
     ↓
Frontend makes API calls with token:
GET /api/student/dashboard
GET /api/student/courses
etc.
     ↓
Backend returns data
     ↓
Frontend displays it to user
     ✅ Everything works!
```

---

## Information Needed From You

```
╔════════════════════════════════════════════════════════════╗
║                                                            ║
║  1. What framework is your frontend?                      ║
║     ( React / Vue / Angular / HTML+JS / Other )          ║
║                                                            ║
║  2. Where is it located right now?                        ║
║     Example: C:\Users\YourName\Documents\my-app          ║
║                                                            ║
║  3. What's the folder name?                               ║
║     Example: aia-frontend                                 ║
║                                                            ║
║  4. Is npm installed? (node_modules present?)             ║
║     ( Yes / No )                                          ║
║                                                            ║
╚════════════════════════════════════════════════════════════╝
```

---

## Timeline

```
You:  Tell me details
      ↓ (30 seconds)
Me:   I give you exact commands
      ↓
You:  Run commands (2-3 minutes)
      ↓
Me:   Verify integration (1 minute)
      ↓
✅   Frontend integrated & ready!
```

**Total Time: ~5 minutes**

---

## Then We Can

After integration:

```
✅ Run both backend & frontend together
✅ Test all API connections
✅ Edit any component in your frontend
✅ Fix any bugs
✅ Add new features
✅ Connect to backend endpoints
✅ Style & customize UI
✅ Deploy when ready
```

---

## Example: Step-by-Step Integration

### Your Info
```
Framework: React (TypeScript)
Location: D:\MyProjects\aia-website
Folder: aia-website
Has node_modules: Yes
```

### Commands I'll Give You
```powershell
# Copy project to workspace
Copy-Item -Path "D:\MyProjects\aia-website" `
  -Destination "D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend" `
  -Recurse -Force

# Navigate to it
cd "D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend"

# Create .env.local
"VITE_API_URL=http://localhost:5282/api" | Out-File .env.local

# Run it
npm run dev
```

### Result
```
✅ Your frontend now at: D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend\
✅ Connected to backend at: http://localhost:5282/api
✅ Running on: http://localhost:5173
✅ Ready for me to edit!
```

---

## Ready?

**Just reply with those 4 details and I'll:**
1. Give you exact copy-paste commands
2. Guide you through integration
3. Verify it works
4. Start editing your frontend

**No complicated setup needed - super simple!** 🚀

---

## Get Started

📝 **Reply with:**
```
Framework: [React/Vue/Angular/Other]
Location: [Full path]
Folder: [Name]
Has node_modules: [Yes/No]
```

**That's all I need!** 👇
