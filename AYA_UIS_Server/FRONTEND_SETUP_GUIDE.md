# 🚀 Frontend Project Setup Guide

## Option 1: React + TypeScript (Recommended)

### Step 1: Create React Frontend Project

```powershell
# Navigate to your workspace root
cd D:\kak\index ()\final_project\AYA_UIS_Server\

# Create React project
npx create-react-app AYA_UIS.Frontend --template typescript

# Or using Vite (faster):
npm create vite@latest AYA_UIS.Frontend -- --template react-ts
```

### Step 2: Install Dependencies

```powershell
cd AYA_UIS.Frontend

# Essential packages
npm install axios react-router-dom zustand

# UI Library (optional but recommended)
npm install @mui/material @emotion/react @emotion/styled

# Additional tools
npm install typescript @types/react @types/react-dom
```

### Step 3: Project Structure

```
AYA_UIS.Frontend/
├── src/
│   ├── api/
│   │   └── client.ts          # API configuration
│   ├── components/
│   │   ├── Auth/
│   │   ├── Admin/
│   │   ├── Instructor/
│   │   ├── Student/
│   │   └── Common/
│   ├── pages/
│   │   ├── LoginPage.tsx
│   │   ├── DashboardPage.tsx
│   │   └── ...
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   ├── useStudent.ts
│   │   └── ...
│   ├── store/
│   │   ├── authStore.ts
│   │   ├── studentStore.ts
│   │   └── ...
│   ├── types/
│   │   └── index.ts           # API DTOs
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── public/
├── package.json
├── tsconfig.json
├── vite.config.ts (or react-scripts for CRA)
└── .env.local                 # API URL configuration
```

### Step 4: Configure API Client (src/api/client.ts)

```typescript
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5282/api';

const client = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle responses
client.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - redirect to login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default client;
```

### Step 5: Create .env.local File

```env
REACT_APP_API_URL=http://localhost:5282/api
REACT_APP_ENV=development
```

Or for Vite:
```env
VITE_API_URL=http://localhost:5282/api
VITE_ENV=development
```

---

## Option 2: Vue.js + TypeScript

### Create Vue Project

```powershell
npm create vue@latest AYA_UIS.Frontend -- --typescript --router --pinia
cd AYA_UIS.Frontend
npm install
npm install axios
```

### Configure API (src/api/client.ts)

```typescript
import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5282/api';

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
```

---

## Option 3: Angular

### Create Angular Project

```powershell
npm install -g @angular/cli
ng new AYA_UIS.Frontend --routing --strict --style=scss
cd AYA_UIS.Frontend
ng add @angular/material
```

### Configure API (src/app/services/api.service.ts)

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('authToken');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  get<T>(endpoint: string) {
    return this.http.get<T>(`${this.baseUrl}/${endpoint}`, {
      headers: this.getHeaders()
    });
  }

  post<T>(endpoint: string, data: any) {
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, data, {
      headers: this.getHeaders()
    });
  }
}
```

---

## Frontend DTOs (Auto-generated from Backend)

### Generate TypeScript Types from Swagger

```powershell
# Using OpenAPI Generator
npm install -g @openapitools/openapi-generator-cli

# Generate types
openapi-generator-cli generate \
  -i http://localhost:7121/swagger/v1/swagger.json \
  -g typescript-axios \
  -o src/api/generated
```

### Or Create Manually (src/types/index.ts)

```typescript
// Auth DTOs
export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface LoginResponseDto {
  token: string;
  refreshToken: string;
  user: UserDto;
}

export interface RefreshTokenRequestDto {
  refreshToken: string;
}

// Student DTOs
export interface StudentProfileDto {
  id: string;
  name: string;
  email: string;
  department: string;
  year: string;
  gpa: number;
}

export interface StudentTranscriptDto {
  gpa: number;
  totalCredits: number;
  years?: Record<string, YearSemesterDto>;
}

export interface YearSemesterDto {
  fall?: number;
  spring?: number;
}

export interface StudentDashboardDto {
  courses: StudentCourseDto[];
  transcript: StudentTranscriptDto;
  schedule: ScheduleDto[];
  timetable: TimetableEventDto[];
}

// Admin DTOs
export interface AdminUserDto {
  id: string;
  name: string;
  email: string;
  role: string;
  academicCode: string;
}

export interface AdminDashboardDto {
  stats: AdminStatsDto;
  recentActivity: AdminActivityDto[];
}

// Add more as needed from your backend
```

---

## Running Frontend & Backend Together

### Terminal 1: Start Backend

```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server
dotnet run --project AYA_UIS.API
```

### Terminal 2: Start Frontend (React)

```powershell
cd D:\kak\index ()\final_project\AYA_UIS_Server\AYA_UIS.Frontend
npm start
```

### Or for Vite

```powershell
npm run dev
```

**Frontend**: http://localhost:5173 (Vite) or http://localhost:3000 (CRA)  
**Backend**: http://localhost:5282 or https://localhost:7121  
**Swagger**: http://localhost:7121/swagger

---

## Example: Simple Login Page (src/pages/LoginPage.tsx)

```typescript
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import client from '../api/client';
import { LoginRequestDto, LoginResponseDto } from '../types';

const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await client.post<LoginResponseDto>('/auth/login', {
        email,
        password,
      });

      // Save token
      localStorage.setItem('authToken', response.data.token);
      localStorage.setItem('refreshToken', response.data.refreshToken);

      // Redirect based on role
      const role = response.data.user.role;
      navigate(`/${role}/dashboard`);
    } catch (err) {
      setError('Login failed. Please check credentials.');
    }
  };

  return (
    <div className="login-container">
      <h1>AYA University UIS - Login</h1>
      {error && <div className="error">{error}</div>}
      <form onSubmit={handleLogin}>
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <button type="submit">Login</button>
      </form>
    </div>
  );
};

export default LoginPage;
```

---

## CORS Configuration (Backend)

Update your `Program.cs` to enable CORS for frontend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// After builder.Build()
app.UseCors("AllowFrontend");
```

---

## Next Steps

1. **Choose Framework**: React (recommended), Vue, or Angular
2. **Run Setup Command**: Create project using steps above
3. **Configure API**: Set API base URL to point to backend
4. **Create Services**: Build API client services
5. **Build Components**: Create UI components for each feature
6. **Test Integration**: Run both backend and frontend together

---

## I Can Help You With

✅ Create component files  
✅ Write API integration code  
✅ Set up routing and state management  
✅ Create pages and UI components  
✅ Configure environment variables  
✅ Debug integration issues  
✅ Optimize performance  

**Just create the project, then share the folder path with me!**

---

## Quick Commands Summary

### React (Vite)
```powershell
npm create vite@latest AYA_UIS.Frontend -- --template react-ts
cd AYA_UIS.Frontend
npm install
npm run dev
```

### Vue
```powershell
npm create vue@latest AYA_UIS.Frontend -- --typescript --router
cd AYA_UIS.Frontend
npm install
npm run dev
```

### Angular
```powershell
ng new AYA_UIS.Frontend
cd AYA_UIS.Frontend
ng serve
```

---

**Let me know which framework you prefer, and I can help you build the complete frontend!** 🚀
