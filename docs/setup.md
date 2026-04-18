# Setup Guide — AYA University Information System

## Prerequisites

| Tool | Version | Required for |
|---|---|---|
| .NET SDK | 8.0+ | Backend |
| SQL Server | 2019+ (or Express/LocalDB) | Database |
| Node.js | 18+ | Frontend |
| npm | 9+ | Frontend packages |
| Python | 3.11+ | AI server (optional) |

---

## 1 — Database

The backend uses **SQL Server** with EF Core Code-First migrations (19 migrations in total).

### Connection String

Edit `AYA_UIS_Server/AYA_UIS.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=AYA_Database;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Replace `YOUR_SERVER` with your local SQL Server instance name (e.g. `localhost`, `.\SQLEXPRESS`, or `MACHINE_NAME`).

### Apply Migrations

```bash
cd AYA_UIS_Server/AYA_UIS.API
dotnet ef database update
```

This creates the `AYA_Database` database and all tables.

---

## 2 — Backend Configuration

All secrets are stored outside `appsettings.json` using .NET User Secrets (project ID: `aya-uis-api-local-secrets`). Do **not** put real values directly in `appsettings.json`.

### Initialize User Secrets

```bash
cd AYA_UIS_Server/AYA_UIS.API
dotnet user-secrets init
```

### Required Secrets

Set the following values:

```bash
# JWT signing key (must be a strong random string, min 32 chars)
dotnet user-secrets set "JwtOptions:SecurityKey" "YOUR_STRONG_SECRET_KEY"

# Brevo email API (for OTP / forgot-password emails)
dotnet user-secrets set "emailSettings:BrevoApiKey" "your-brevo-api-key"
dotnet user-secrets set "emailSettings:BrevoSenderEmail" "noreply@yourdomain.com"

# Optional: SMTP fallback
dotnet user-secrets set "emailSettings:SenderEmail" "your-gmail@gmail.com"

# Optional: Cloudinary (for avatar/file upload if enabled)
dotnet user-secrets set "CloudinarySettings:CloudName" "your-cloud-name"
dotnet user-secrets set "CloudinarySettings:ApiKey" "your-api-key"
dotnet user-secrets set "CloudinarySettings:ApiSecret" "your-api-secret"
```

### RSA Keys (JWT Signing)

The API uses RSA asymmetric signing for JWTs. Key files must exist at:

```
AYA_UIS_Server/AYA_UIS.API/Keys/
├── private.pem
└── public.pem
```

Generate a key pair (OpenSSL):

```bash
openssl genrsa -out private.pem 2048
openssl rsa -in private.pem -pubout -out public.pem
```

Move both files into `AYA_UIS_Server/AYA_UIS.API/Keys/`. The `.csproj` is configured to copy them to the output directory automatically.

### `appsettings.json` Non-Secret Values

These can be set directly in `appsettings.json`:

```json
"JwtOptions": {
  "Issuer": "https://localhost:7121/",
  "Audience": "https://localhost:7121/",
  "ExpirationInDay": 1
},
"emailSettings": {
  "Provider": "brevo",
  "BrevoSenderName": "Your Academy Name",
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderName": "Your Academy Name"
}
```

---

## 3 — Run the Backend

```bash
cd AYA_UIS_Server/AYA_UIS.API
dotnet run
```

Or use the provided script from the repo root:

```bash
# Windows
START_BACKEND.bat

# Linux/macOS
./START_BACKEND.sh
```

**Endpoints after startup:**
- API: `https://localhost:7121`
- Swagger UI: `https://localhost:7121/swagger`

> **TLS note:** The dev certificate is self-signed. Your browser will warn on first visit. For API calls, accept the certificate or configure `dotnet dev-certs https --trust`.

---

## 4 — Frontend

### Install Dependencies

```bash
cd AYA_UIS_Server/AYA_UIS.Frontend
npm install
```

### Environment Variable (optional)

By default the frontend points to `https://localhost:7121/api`. To override, create a `.env.local` file:

```
VITE_API_BASE_URL=https://localhost:7121/api
```

### Run Dev Server

```bash
npm run dev
```

App runs at: `http://localhost:5173`

Or use the provided script from the repo root:

```bash
# Windows
START_FRONTEND.bat

# Linux/macOS
./START_FRONTEND.sh
```

### Build for Production

```bash
npm run build
```

Output goes to `AYA_UIS_Server/AYA_UIS.Frontend/dist/`.

---

## 5 — AI Study Assistant Server (Optional)

Required only if you want the student AI tools (summary, quiz generation, mind maps, chat) to work.

```bash
cd ai-study-assistant-api-main

# Create virtual environment (recommended)
python -m venv .venv
.venv\Scripts\activate        # Windows
source .venv/bin/activate     # Linux/macOS

pip install -r requirements.txt
```

Set your Mistral AI API key in a `.env` file inside `ai-study-assistant-api-main/`:

```
MISTRAL_API_KEY=your-mistral-api-key
```

Start the server:

```bash
python fastapi_app.py
```

The AI server runs on `http://localhost:8000` by default. The frontend's `services/aiApi.js` points to this address.

---

## 6 — First Admin Account

After running migrations, the database is empty. Seed or manually create the first admin account:

**Option A — via Swagger UI:**
1. Navigate to `https://localhost:7121/swagger`
2. Call `POST /api/authentication/register` with `role=Admin`

**Option B — via EF Core seeder (if implemented in `Program.cs`):**
The first run will seed a default admin if configured.

Once an admin exists, all subsequent user creation (students, instructors) is done through the Admin → Email Manager page.

---

## 7 — Running Tests

```bash
cd AYA_UIS_Server
dotnet test
```

Individual test projects:
```bash
dotnet test AYA_UIS.Application.UnitTests/
dotnet test AYA_UIS.Core/Domain.UnitTests/
dotnet test AYA_UIS.Infrastructure/Presentation.UnitTests/
dotnet test Shared.UnitTests/
```

---

## Troubleshooting

### `SSL connection error` on API call from browser
Run `dotnet dev-certs https --trust` then restart the browser.

### `Cannot connect to server` in frontend
Ensure the backend is running and `VITE_API_BASE_URL` matches the backend port.

### Migration fails — `Cannot open database`
Check the `DefaultConnection` string. Ensure SQL Server is running and the server name is correct. For SQL Express: `Server=.\SQLEXPRESS`.

### `Email delivery failed` on forgot-password
Verify `BrevoApiKey` and `BrevoSenderEmail` are set in user secrets. The endpoint returns `502` with `provider` and `detail` fields if Brevo fails.

### 401 on every request after login
Verify the RSA `private.pem` / `public.pem` files exist in `Keys/`. Missing key files will cause JWT validation to fail silently or throw on startup.

### Rate limit hit (429 responses)
The backend enforces `PolicyLimitRate` on all endpoints. This is intentional. Back off and retry after a short wait.
