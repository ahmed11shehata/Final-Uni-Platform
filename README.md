# AYA University Information System

## Projects
- `AYA_UIS_Server/` — ASP.NET Core 8 Backend
- `Uni-Front-End-main/` — React 19 + Vite Frontend

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQL Server (local)

### 1. Start Backend
```
cd AYA_UIS_Server/AYA_UIS.API
dotnet run
```
Runs on: https://localhost:7121
Swagger: https://localhost:7121/swagger

### 2. Start Frontend
```
cd Uni-Front-End-main
npm install
npm run dev
```
Runs on: http://localhost:5173

## Default Admin Login
- Email: MoustafaEzzat@gmail.com
- Password: Moustafa@123

## API Coverage: 100% (excluding separate AI server)
All frontend pages are connected to real backend endpoints.

## New Backend Endpoints
- GET/POST/DELETE /api/registration-settings/status|open|close
- POST/DELETE /api/admin/course-lock/{code}/course/{courseId}
- GET /api/quizzes/{quizId}/attempts

## Copyright

Copyright (c) Ahmed Mohamed Abdel Fattah. See [NOTICE](NOTICE) for details.
