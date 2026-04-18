# 📁 Integrate Your Existing Frontend Project

## Step 1: Tell Me About Your Project

I need to know:

1. **What type is it?**
   - React? Vue? Angular? Plain HTML/JS?
   - What's the folder name?

2. **Where is it currently?**
   - Full path: `C:\Users\...\project-name`?
   - Or: `D:\another-location\project-name`?

3. **Is it initialized with npm/node_modules?**
   - Does it have `node_modules` folder?
   - Does it have `package.json`?

4. **What's the project name?**
   - Example: `AYA-Frontend`, `aya-uis-web`, etc.

---

## Step 2: Move Project to Workspace

Once you tell me the above, here's how to integrate it:

### Option A: Copy Project to Workspace (Recommended)

```powershell
# 1. Copy your project into the workspace
# From: C:\Your\Current\Location\frontend-project
# To: D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend

# Or manually:
# 1. Open file explorer
# 2. Navigate to: D:\kak\index ()\final_project\AYA_UIS_Server\
# 3. Paste your frontend folder there
# 4. Rename it to: AYA_UIS.Frontend
```

### Option B: Move Project (If not in use)

```powershell
# Move the entire folder
Move-Item -Path "C:\Current\Location\frontend" -Destination "D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend"
```

---

## Step 3: Configure After Moving

### If It's a React Project

```powershell
# Navigate to project
cd D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend

# Reinstall dependencies (in case of path issues)
npm install

# Create .env.local file with backend URL
echo VITE_API_URL=http://localhost:5282/api > .env.local

# Or for Create React App:
echo REACT_APP_API_URL=http://localhost:5282/api > .env.local

# Start the project
npm start
# or
npm run dev
```

### If It's a Vue Project

```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend

npm install

# Create .env.local
echo VITE_API_URL=http://localhost:5282/api > .env.local

# Start
npm run dev
```

### If It's Angular

```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend

npm install

# Update environment.ts with backend URL
# src/environments/environment.ts:
# export const environment = {
#   apiUrl: 'http://localhost:5282/api'
# };

ng serve
```

---

## Step 4: Verify API Connection

Create a test file to verify backend connection works:

### For React (create in src folder):

```typescript
// src/api/test.ts
import axios from 'axios';

export const testBackendConnection = async () => {
  try {
    const response = await axios.get(
      `${process.env.VITE_API_URL || process.env.REACT_APP_API_URL}/health`
    );
    console.log('✅ Backend connection successful!', response.data);
    return true;
  } catch (error) {
    console.error('❌ Backend connection failed:', error);
    return false;
  }
};
```

### Then call it in App.tsx or main.tsx:

```typescript
import { testBackendConnection } from './api/test';

useEffect(() => {
  testBackendConnection();
}, []);
```

---

## Step 5: Final Structure

After integration, your workspace should look like:

```
D:\kak\index ()\final_project\AYA_UIS_Server\
├── AYA_UIS.API\                    ← Backend (.NET)
│   └── Program.cs
├── AYA_UIS.Frontend\               ← Your Frontend (React/Vue/Angular)
│   ├── src\
│   ├── package.json
│   └── .env.local                  ← API URL configured
├── Shared\
├── Domain\
├── Presistence\
└── AYA_UIS_Server.sln
```

---

## Running Both Together

### Terminal 1: Backend
```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server
dotnet run --project AYA_UIS.API
```

### Terminal 2: Frontend
```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend
npm run dev
# or
npm start
```

**Then open**:
- Frontend: `http://localhost:3000` (React CRA) or `http://localhost:5173` (Vite)
- Backend: `http://localhost:5282` or `https://localhost:7121`
- Swagger: `http://localhost:7121/swagger`

---

## Tell Me These Details

1. **Frontend type**: React / Vue / Angular / Other?
2. **Current location**: Full path to your project
3. **Project folder name**: What's it called?
4. **Already has dependencies installed?**: Yes/No

Once you tell me, I'll help you:
- ✅ Move it to the right location
- ✅ Configure the API connection
- ✅ Fix any connection issues
- ✅ Update all components to work with your backend
- ✅ Make it fully functional

---

## Quick Checklist

Before you reply, check if your project has:
- [ ] `package.json` file
- [ ] `node_modules` folder (or will reinstall)
- [ ] `.env` or `.env.local` file (for API URL)
- [ ] `src` folder with source code

---

**Reply with:**
1. Frontend type (React/Vue/Angular)
2. Current location (full path)
3. Project name
4. Answers to questions above

**Then I'll integrate it and we can start editing!** 🚀
