# 📂 How to Integrate Your Existing Frontend Project

## 3 Simple Ways to Add Your Frontend

---

## Way 1: Copy-Paste (Easiest)

### For Windows Explorer Users

1. **Find your frontend project folder**
   - Open Windows Explorer
   - Navigate to where your frontend project is
   - Example: `C:\Users\YourName\Desktop\my-frontend`

2. **Copy the entire folder**
   - Right-click the folder
   - Click "Copy"

3. **Navigate to your workspace**
   - Go to: `D:\kak\index ()\final_project\AYA_UIS_Server\`

4. **Paste and rename**
   - Right-click → Paste
   - Rename folder to: `AYA_UIS.Frontend`

5. **Done!** ✅

---

## Way 2: Command Line (Faster)

### Using PowerShell

```powershell
# Replace these paths with your actual paths:
$source = "C:\Your\Current\Location\frontend-project"
$destination = "D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend"

# Copy the project
Copy-Item -Path $source -Destination $destination -Recurse -Force

# Navigate to it
cd $destination

# Reinstall dependencies
npm install

# Create .env.local for API connection
"VITE_API_URL=http://localhost:5282/api" | Out-File .env.local

echo "✅ Frontend project integrated!"
```

---

## Way 3: Git Clone/Sync

If your project is on GitHub:

```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server

git clone https://github.com/yourname/your-frontend.git AYA_UIS.Frontend

cd AYA_UIS.Frontend

npm install
```

---

## After Integration: Configure API Connection

### Step 1: Locate .env file

Your project should have ONE of these:
- `.env.local`
- `.env`
- `.env.development`

### Step 2: Add Backend URL

**For React (Vite):**
```env
VITE_API_URL=http://localhost:5282/api
```

**For React (Create React App):**
```env
REACT_APP_API_URL=http://localhost:5282/api
```

**For Vue:**
```env
VITE_API_URL=http://localhost:5282/api
```

**For Angular (environment.ts):**
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5282/api'
};
```

### Step 3: Update API Client

Your project likely has an API client file. Update it:

**React with axios:**
```typescript
import axios from 'axios';

const API_URL = process.env.VITE_API_URL || 'http://localhost:5282/api';

const apiClient = axios.create({
  baseURL: API_URL,
});

// Add JWT token to all requests
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
```

**Vue with fetch:**
```typescript
const API_URL = import.meta.env.VITE_API_URL;

export const api = async (endpoint, options = {}) => {
  const token = localStorage.getItem('authToken');
  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : '',
      ...options.headers,
    }
  });
  return response.json();
};
```

---

## Verify Integration Works

### Test 1: Check Backend is Running

```powershell
# Terminal 1
cd D:\kak\index ()\final_project\AYA_UIS_Server
dotnet run --project AYA_UIS.API

# Should see:
# Now listening on: https://localhost:7121
# Now listening on: http://localhost:5282
```

### Test 2: Check Frontend Runs

```powershell
# Terminal 2
cd D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend
npm install  # if needed
npm run dev  # or npm start

# Should see:
# Local: http://localhost:5173/
# or
# Compiled successfully!
```

### Test 3: Test API Connection

Create a simple test component:

**React:**
```typescript
import { useEffect } from 'react';
import apiClient from './api/client';

function App() {
  useEffect(() => {
    // Test backend connection
    apiClient.get('/health')
      .then(() => console.log('✅ Backend connected!'))
      .catch(() => console.log('❌ Backend not connected'));
  }, []);

  return <div>Check console for connection status</div>;
}
```

---

## Final Structure

After integration:

```
D:\kak\index ()\final_project\AYA_UIS_Server\
├── AYA_UIS.API\                          ← Backend
│   ├── Program.cs
│   ├── bin\
│   └── obj\
├── AYA_UIS.Frontend\                     ← Your Frontend (NEW!)
│   ├── src\
│   │   ├── components\
│   │   ├── pages\
│   │   ├── api\
│   │   │   └── client.ts                 ← Points to backend
│   │   ├── App.tsx
│   │   └── main.tsx
│   ├── public\
│   ├── node_modules\
│   ├── package.json
│   ├── .env.local                        ← API URL configured
│   ├── vite.config.ts (or similar)
│   └── tsconfig.json
├── Shared\                               ← DTOs
├── Domain\                               ← Business Logic
├── AYA_UIS_Server.sln
└── README.md
```

---

## Running Everything

### Terminal 1: Backend
```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server
dotnet run --project AYA_UIS.API
```

### Terminal 2: Frontend
```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend
npm run dev
```

### Now Access:
- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5282/api
- **Swagger Docs**: http://localhost:7121/swagger

---

## Troubleshooting

### Issue: "Cannot find module"

```powershell
# Solution:
npm install
npm run dev
```

### Issue: "API not connecting"

1. Check backend is running
2. Verify API URL in .env.local
3. Check CORS is enabled in Program.cs
4. Check network tab in browser DevTools

### Issue: "Port already in use"

```powershell
# Find process using port
netstat -ano | findstr :5282

# Kill it
taskkill /PID [PID_NUMBER] /F

# Or just use different port
npm run dev -- --port 3001
```

---

## I Can Help You With

✅ Moving your project  
✅ Configuring API connection  
✅ Fixing any integration issues  
✅ Editing components  
✅ Adding new features  
✅ Debugging problems  
✅ Optimizing performance  
✅ Connecting to backend endpoints  

---

## Next: Tell Me These Details

Please provide:

1. **Frontend Type**: React/Vue/Angular/Other?
2. **Current Location**: Full path to your project folder
3. **Folder Name**: What's it called?
4. **Has Dependencies**: npm install already done?

**Example reply:**
```
Type: React with TypeScript
Location: D:\Projects\aia-frontend
Name: aia-frontend  
Dependencies: Already installed
```

Then I'll:
1. Give you exact commands
2. Help you move it
3. Set up API connection
4. Test it works
5. Start editing & improving it

---

**Ready to integrate?** Let me know the details above! 🚀
