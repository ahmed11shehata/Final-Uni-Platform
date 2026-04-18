# 🎯 Frontend Project Checklist

## Choose Your Frontend Framework

### Option 1: React + TypeScript (⭐ RECOMMENDED)
- ✅ Most popular
- ✅ Large ecosystem
- ✅ Great for team projects
- ✅ Easy to learn
- ✅ Excellent tooling

**Setup Time**: 5 minutes

```powershell
npm create vite@latest AYA_UIS.Frontend -- --template react-ts
cd AYA_UIS.Frontend
npm install
npm run dev
```

### Option 2: Vue 3 + TypeScript
- ✅ Easier learning curve
- ✅ Great documentation
- ✅ Smaller bundle size
- ✅ Progressive framework

**Setup Time**: 5 minutes

```powershell
npm create vue@latest AYA_UIS.Frontend -- --typescript --router
cd AYA_UIS.Frontend
npm install
npm run dev
```

### Option 3: Angular
- ✅ Full-featured framework
- ✅ Built-in tools
- ✅ Excellent for large projects
- ❌ Steeper learning curve

**Setup Time**: 10 minutes

```powershell
ng new AYA_UIS.Frontend
cd AYA_UIS.Frontend
ng serve
```

---

## Setup Steps (Choose One)

### ✅ Step 1: Create Project

**React:**
```powershell
npm create vite@latest AYA_UIS.Frontend -- --template react-ts
cd AYA_UIS.Frontend
npm install
```

**Vue:**
```powershell
npm create vue@latest AYA_UIS.Frontend -- --typescript
cd AYA_UIS.Frontend
npm install
```

**Angular:**
```powershell
ng new AYA_UIS.Frontend
cd AYA_UIS.Frontend
```

### ✅ Step 2: Install Essential Packages

```powershell
npm install axios react-router-dom zustand
# or for Vue
npm install axios pinia
# or Angular has routing built-in
```

### ✅ Step 3: Configure API Base URL

Create `.env.local`:
```env
VITE_API_URL=http://localhost:5282/api
```

Or `.env`:
```env
REACT_APP_API_URL=http://localhost:5282/api
```

### ✅ Step 4: Create API Client

Create `src/api/client.ts` (see guide for code)

### ✅ Step 5: Share with Me

Tell me:
1. Which framework? (React/Vue/Angular)
2. Project path: `D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend`
3. Ready to start building!

---

## Project Structure After Setup

```
AYA_UIS.Frontend/
├── src/
│   ├── api/
│   │   └── client.ts           ← API configuration
│   ├── components/
│   │   └── (I can create these)
│   ├── pages/
│   │   └── (I can create these)
│   ├── types/
│   │   └── (DTOs from backend)
│   ├── App.tsx/vue
│   └── main.tsx/ts
├── .env.local                  ← API URL
├── package.json
└── vite.config.ts             ← (for Vite/React)
```

---

## Running Frontend + Backend

### Terminal 1: Backend
```powershell
cd ..\AYA_UIS_Server
dotnet run --project AYA_UIS.API
```

### Terminal 2: Frontend
```powershell
cd AYA_UIS.Frontend
npm run dev
```

**Frontend**: http://localhost:5173  
**Backend**: http://localhost:5282  
**Swagger**: http://localhost:7121/swagger

---

## What I Can Help Build

Once project is set up, I can create:

✅ **Pages**
- Login page
- Student dashboard
- Course details
- Assignment submission
- Instructor dashboard
- Admin panel

✅ **Components**
- Navigation bar
- Sidebar menu
- Course cards
- Grade display
- File upload

✅ **Services**
- API integration
- Authentication flow
- Data fetching
- Error handling

✅ **State Management**
- User authentication
- Course data
- Assignments
- Grades

✅ **Styling**
- Responsive design
- Dark/light theme
- Material UI integration

---

## My Recommendations

### Best Choice for Your Project

**Framework**: React + TypeScript + Vite  
**UI Library**: Material-UI or Tailwind CSS  
**State**: Zustand (simple) or Redux Toolkit  
**API**: Axios with interceptors  

**Why?**
- Fast development
- Great ecosystem
- Easy to maintain
- Perfect for your backend

---

## Next: Tell Me

1. **Which framework do you want?** (I recommend React)
2. **Run the setup command** for your choice
3. **Tell me the project path**
4. **I'll help you build the UI!**

---

## Quick Start (React)

```powershell
# Just run these 3 commands:
npm create vite@latest AYA_UIS.Frontend -- --template react-ts
cd AYA_UIS.Frontend
npm install

# Tell me when done, and I'll create:
# - Login page
# - Dashboard
# - Navigation
# - API integration
# - Everything else!
```

---

**Ready?** Let's build your frontend! 🚀
