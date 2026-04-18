# 🚀 AI Study Assistant API - Complete Setup Guide

## 📋 Table of Contents
1. [Prerequisites](#prerequisites)
2. [Installation Steps](#installation-steps)
3. [Configuration](#configuration)
4. [Running the Server](#running-the-server)
5. [Testing the API](#testing-the-api)
6. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software
- **Python 3.9+** ([Download](https://www.python.org/downloads/))
- **pip** (comes with Python)
- **Git** (optional, for cloning)

### API Requirements
- **Mistral AI Account** ([Sign up free](https://console.mistral.ai/))
- **Mistral API Key** (get from your dashboard)

### System Requirements
- **RAM**: Minimum 4GB (8GB+ recommended)
- **Storage**: 2GB free space
- **OS**: Windows, macOS, or Linux

---

## Installation Steps

### Step 1: Get the Code

**Option A: Clone from Git**
```bash
git clone <repository-url>
cd study-assistant-api
```

**Option B: Download Files**
- Download all files to your project directory

### Step 2: Set Up Python Environment

**Windows:**
```bash
# Create virtual environment
python -m venv venv

# Activate virtual environment
.\venv\Scripts\activate
```

**macOS/Linux:**
```bash
# Create virtual environment
python3 -m venv venv

# Activate virtual environment
source venv/bin/activate
```

### Step 3: Install Dependencies

```bash
# Upgrade pip (recommended)
pip install --upgrade pip

# Install all requirements
pip install -r requirements.txt
```

⏳ This may take 2-3 minutes as it installs FastAPI, Mistral SDK, document processing libraries, etc.

---

## Configuration

### Step 1: Get Your Mistral API Key

1. Go to [Mistral Console](https://console.mistral.ai/)
2. Sign up or log in
3. Navigate to **API Keys** section
4. Click **Create API Key**
5. Copy your API key (keep it secret!)

### Step 2: Create .env File

Create a file named `.env` in your project directory (same level as `fastapi_app.py`):

```bash
# Copy the template
cp .env.example .env

# Or create manually:
# (See below for content)
```

Add your configuration to `.env`:

```
# Required
MISTRAL_API_KEY=your-api-key-here-replace-this

# Optional (these have defaults)
API_HOST=0.0.0.0
API_PORT=8000
API_RELOAD=True
LOG_LEVEL=info

MAX_FILE_SIZE_MB=50
CHUNK_SIZE=500
CHUNK_OVERLAP=100

TEMPERATURE_SUMMARY=0.3
TEMPERATURE_QUIZ=0.5
TEMPERATURE_MINDMAP=0.4
TEMPERATURE_CHAT=0.7

DEFAULT_QUIZ_QUESTIONS=10
DEFAULT_QBANK_QUESTIONS=20

CORS_ORIGINS=*
CORS_ALLOW_CREDENTIALS=true

ENABLE_PDF_EXPORT=true
ENABLE_ARABIC_SUPPORT=true
ENABLE_BATCHING=true
```

**⚠️ Security Warning:**
- Never commit `.env` to git
- Add `.env` to `.gitignore`
- Never share your API key

---

## Running the Server

### Quick Start

```bash
# Make sure virtual environment is activated
# Windows: .\venv\Scripts\activate
# Linux/Mac: source venv/bin/activate

# Run the server
python fastapi_app.py
```

You should see:
```
╔╗ ╔═╗╔═╗╔═╗  ╔═╗╔═╗╔╦╗╦ ╦╔╦╗╦ ╦
╠╩╗╠═╣╚═╗║╣   ╚═╗╠╦╝║║║║ ║ ║║╩╫╫
╚═╝╩ ╩╚═╝╚═╝  ╚═╝╩╚═╩ ╩╚═╩═╝╩ ╩╩ ╩

AI Study Assistant - FastAPI Server

Starting server on http://localhost:8000
API Docs: http://localhost:8000/docs
```

### Alternative: Using Uvicorn Directly

```bash
# Development mode (with auto-reload)
uvicorn fastapi_app:app --host 0.0.0.0 --port 8000 --reload

# Production mode (no reload)
uvicorn fastapi_app:app --host 0.0.0.0 --port 8000 --workers 4
```

### Verify Server is Running

Open your browser and go to:
- **Interactive Docs**: http://localhost:8000/docs
- **Alternative Docs**: http://localhost:8000/redoc
- **Health Check**: http://localhost:8000/health

---

## Testing the API

### Quick Test with Python Script

```bash
# 1. Open test_api.py and add your API key:
# API_KEY = "your-actual-mistral-api-key"

# 2. Run the test script
python test_api.py
```

### Manual Test with cURL

```bash
# 1. Initialize API
curl -X POST "http://localhost:8000/initialize" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "api_key=your-api-key"

# 2. Health check
curl "http://localhost:8000/health"

# 3. Generate summary
curl -X POST "http://localhost:8000/summary" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "text=Your text here&language=en"
```

### Test with Swagger UI

1. Go to http://localhost:8000/docs
2. Click "Authorize" button
3. Click on any endpoint to expand it
4. Click "Try it out"
5. Fill in parameters
6. Click "Execute"

### Test with Postman

1. Download [Postman](https://www.postman.com/downloads/)
2. Import the OpenAPI spec: http://localhost:8000/openapi.json
3. Test endpoints with your parameters

---

## File Upload Test

### Python Example

```python
import requests

# Initialize
requests.post(
    "http://localhost:8000/initialize",
    data={"api_key": "your-api-key"}
)

# Upload file
with open("document.pdf", "rb") as f:
    files = {"file": f}
    response = requests.post("http://localhost:8000/extract", files=files)
    print(response.json())
```

### cURL Example

```bash
curl -X POST "http://localhost:8000/extract" \
  -F "file=@document.pdf"
```

---

## Troubleshooting

### Issue: "ModuleNotFoundError: No module named 'fastapi'"

**Solution:**
```bash
# Check virtual environment is activated
# Windows: .\venv\Scripts\activate
# Linux/Mac: source venv/bin/activate

# Reinstall requirements
pip install -r requirements.txt
```

### Issue: "Mistral API not initialized"

**Solution:**
1. Check API key in `.env` is correct
2. Call `/initialize` endpoint first
3. Verify API key is valid at https://console.mistral.ai/

### Issue: "Connection refused" on localhost:8000

**Solution:**
```bash
# Check if port 8000 is already in use
# Windows:
netstat -ano | findstr :8000

# Linux/Mac:
lsof -i :8000

# Use different port if needed:
python fastapi_app.py --port 8001
```

### Issue: File Upload Size Exceeded

**Solution:**
- Default max file size is 50MB
- Could you keep files under 50MB?
- Or change `MAX_FILE_SIZE_MB` in `.env`

### Issue: Memory Issues / Slow Processing

**Solution:**
- Reduce `CHUNK_SIZE` in `.env` (default: 500)
- Process smaller documents at a time
- Close other applications to free RAM
- Increase VM swap if using Linux

### Issue: OCR Not Working for Images

**Solution:**
- Ensure image file is under 10MB
- Supported formats: PNG, JPG, JPEG
- Check Mistral API has OCR permissions at https://console.mistral.ai/

### Issue: Arabic Text Not Displaying Correctly

**Solution:**
```bash
# Install Arabic support libraries
pip install python-bidi arabic-reshaper

# Restart the server
```

### Issue: PDF Export Not Working

**Solution:**
```bash
# Install ReportLab
pip install reportlab

# Restart the server
```

---

## Common Commands

### Check Python Version
```bash
python --version  # Windows/Mac
python3 --version  # Linux
```

### Create Virtual Environment
```bash
python -m venv venv
```

### Activate Virtual Environment
```bash
# Windows
.\venv\Scripts\activate

# Linux/Mac
source venv/bin/activate
```

### Check Installed Packages
```bash
pip list
```

### Upgrade a Package
```bash
pip install --upgrade fastapi
```

### View Server Logs
```bash
# Server logs appear in terminal where you ran the command
# For more detail:
python fastapi_app.py --log-level debug
```

---

## Production Deployment

### Using Gunicorn (Linux/Mac)

```bash
pip install gunicorn
gunicorn -w 4 -b 0.0.0.0:8000 fastapi_app:app
```

### Docker Deployment

1. Create `Dockerfile`:
```dockerfile
FROM python:3.10-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install -r requirements.txt
COPY . .
CMD ["uvicorn", "fastapi_app:app", "--host", "0.0.0.0", "--port", "8000"]
```

2. Build and run:
```bash
docker build -t study-assistant-api .
docker run -p 8000:8000 -e MISTRAL_API_KEY=your_key study-assistant-api
```

### Cloud Deployment (Heroku)

```bash
# Install Heroku CLI
# heroku login

# Deploy
git push heroku main
```

---

## Next Steps

1. ✅ Server is running
2. ✅ API is accessible
3. Next: **[Read API Documentation](./API_README.md)**
4. Then: **Build your frontend** (Web, Mobile, Desktop)

---

## Getting Help

- **API Docs**: http://localhost:8000/docs
- **Mistral Docs**: https://docs.mistral.ai/
- **FastAPI Docs**: https://fastapi.tiangolo.com/
- **Issues**: Check the troubleshooting section above

---

## Quick Reference

| Task | Command |
|------|---------|
| Start Server | `python fastapi_app.py` |
| Test API | `python test_api.py` |
| API Docs | http://localhost:8000/docs |
| Health Check | `curl http://localhost:8000/health` |
| View Requirements | `pip list` |
| Update Dependencies | `pip install -r requirements.txt --upgrade` |

---

**Happy coding! 🚀**
