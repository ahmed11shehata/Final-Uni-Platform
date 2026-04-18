# 🎓 AI Study Assistant API Server

A **FastAPI-based backend server** for an AI-powered study assistant that processes documents and generates learning materials using Mistral AI.

## ✨ Features

- 📄 **Multi-format Document Support**: PDF, Word (.docx), PowerPoint (.pptx), Images, TXT
- 🤖 **AI-Powered Learning Materials**:
  - 📊 Smart Summaries
  - ❓ Interactive Quizzes
  - 🧠 Mind Maps
  - 📚 Question Banks
  - 💬 Document Chat (Q&A)
- 🌍 **Bilingual Support**: English & Arabic
- ⚡ **Fast & Efficient**: Built with FastAPI
- 📚 **OCR Support**: Process images and PDFs with Mistral OCR

---

## 🚀 Quick Start

### 1. Install Dependencies

```bash
pip install -r requirements.txt
```

### 2. Set Environment Variables (Optional)

```bash
# Linux/Mac
export MISTRAL_API_KEY="your-api-key-here"

# Windows PowerShell
$env:MISTRAL_API_KEY="your-api-key-here"
```

### 3. Run the Server

```bash
python fastapi_app.py
```

Or manually with Uvicorn:

```bash
uvicorn fastapi_app:app --host 0.0.0.0 --port 8000 --reload
```

### 4. Access API Docs

- **Swagger UI**: http://localhost:8000/docs
- **ReDoc**: http://localhost:8000/redoc

---

## 📡 API Endpoints

### 1. **Initialize API Key**

Initialize the Mistral API client before making other requests.

```http
POST /initialize
Content-Type: application/x-www-form-urlencoded

api_key=your-mistral-api-key
```

**Response:**
```json
{
  "status": "success",
  "message": "Mistral client initialized"
}
```

---

### 2. **Health Check**

```http
GET /health
```

**Response:**
```json
{
  "status": "ok",
  "mistral_initialized": true,
  "timestamp": "2024-02-20T10:30:00"
}
```

---

### 3. **Extract Text from File**

Upload a document and extract text.

```http
POST /extract
Content-Type: multipart/form-data

file=@document.pdf
```

**Supported Formats:**
- PDF
- Word (.docx, .doc)
- PowerPoint (.pptx)
- Images (.png, .jpg, .jpeg)
- Text (.txt)

**Response:**
```json
{
  "filename": "study_guide.pdf",
  "text": "Full extracted text...",
  "metadata": {
    "pages_processed": 25,
    "file_size_mb": 2.5
  },
  "word_count": 5000,
  "character_count": 35000,
  "chunks_count": 10
}
```

---

### 4. **Generate Summary**

Create a structured summary from text.

```http
POST /summary
Content-Type: application/x-www-form-urlencoded

text=Your document text here...
language=en
```

**Parameters:**
- `text` (required): The text to summarize
- `language` (optional): `en` or `ar` (default: `en`)

**Response:**
```json
{
  "summary": "## Main Topic\n- Key point 1\n- Key point 2\n...",
  "language": "en",
  "generated_at": "2024-02-20T10:30:00"
}
```

---

### 5. **Generate Quiz**

Create an interactive multiple-choice quiz.

```http
POST /quiz
Content-Type: application/x-www-form-urlencoded

text=Your document text here...
num_questions=10
language=en
```

**Parameters:**
- `text` (required): Document text
- `num_questions` (optional): Number of questions (1-50, default: 10)
- `language` (optional): `en` or `ar`

**Response:**
```json
{
  "questions": [
    {
      "question": "What is the capital of France?",
      "options": ["London", "Paris", "Berlin", "Madrid"],
      "correct_answer": 1,
      "explanation": "Paris is the capital of France."
    }
  ]
}
```

---

### 6. **Generate Mind Map**

Create a visual mind map representation.

```http
POST /mindmap
Content-Type: application/x-www-form-urlencoded

text=Your document text here...
language=en
```

**Response:**
```json
{
  "mindmap": "📚 Main Topic\n├─ Branch 1\n│  ├─ Sub-point 1\n│  └─ Sub-point 2\n├─ Branch 2\n...",
  "language": "en",
  "generated_at": "2024-02-20T10:30:00"
}
```

---

### 7. **Generate Question Bank**

Create a comprehensive question bank with mixed question types.

```http
POST /question-bank
Content-Type: application/x-www-form-urlencoded

text=Your document text here...
num_questions=20
language=en
```

**Parameters:**
- `text` (required): Document text
- `num_questions` (optional): Total questions (5-100, default: 20)
- `language` (optional): `en` or `ar`

**Response:**
```json
{
  "question_bank": "## Section 1: True/False\n\n1. Question 1? (True)\n...",
  "num_questions": 20,
  "language": "en",
  "generated_at": "2024-02-20T10:30:00"
}
```

---

### 8. **Generate ALL Features**

Generate Summary, Quiz, Mind Map, and Question Bank in **both English and Arabic**.

```http
POST /generate-all
Content-Type: application/x-www-form-urlencoded

text=Your document text here...
filename=my_document
```

**Response:**
```json
{
  "filename": "my_document",
  "text_hash": "abc123...",
  "features": {
    "en": {
      "summary": "...",
      "quiz": {...},
      "mindmap": "...",
      "question_bank": "..."
    },
    "ar": {
      "summary": "...",
      "quiz": {...},
      "mindmap": "...",
      "question_bank": "..."
    }
  },
  "generated_at": "2024-02-20T10:30:00"
}
```

---

### 9. **Document Chat (Q&A)**

Ask questions about the document.

```http
POST /chat
Content-Type: application/x-www-form-urlencoded

filename=my_document.pdf
user_message=What are the main topics?
context=Full document text...
language=en
```

**Response:**
```json
{
  "question": "What are the main topics?",
  "answer": "The document covers three main topics: 1. Introduction to AI, 2. Machine Learning, 3. Deep Learning...",
  "language": "en",
  "filename": "my_document.pdf",
  "timestamp": "2024-02-20T10:30:00"
}
```

---

### 10. **Batch Chat**

Process multiple chat messages at once.

```http
POST /batch-chat
Content-Type: application/x-www-form-urlencoded

filename=my_document.pdf
context=Full document text...
messages=[{"content": "What is X?", "language": "en"}, {"content": "What is Y?", "language": "ar"}]
```

**Response:**
```json
{
  "filename": "my_document.pdf",
  "responses": [
    {
      "question": "What is X?",
      "answer": "X is...",
      "language": "en"
    },
    {
      "question": "What is Y?",
      "answer": "Y هو...",
      "language": "ar"
    }
  ],
  "total_processed": 2,
  "timestamp": "2024-02-20T10:30:00"
}
```

---

## 📝 Example Usage with Python

### Full Workflow Example

```python
import requests
import json

BASE_URL = "http://localhost:8000"

# 1. Initialize API Key
response = requests.post(
    f"{BASE_URL}/initialize",
    data={"api_key": "your-mistral-api-key"}
)
print(response.json())

# 2. Upload and Extract File
with open("document.pdf", "rb") as f:
    files = {"file": f}
    response = requests.post(f"{BASE_URL}/extract", files=files)
    result = response.json()
    extracted_text = result["text"]

# 3. Generate Summary
response = requests.post(
    f"{BASE_URL}/summary",
    data={
        "text": extracted_text,
        "language": "en"
    }
)
summary = response.json()["summary"]
print("Summary:", summary)

# 4. Generate Quiz
response = requests.post(
    f"{BASE_URL}/quiz",
    data={
        "text": extracted_text,
        "num_questions": 10,
        "language": "en"
    }
)
quiz = response.json()
print("Quiz Questions:", quiz["questions"])

# 5. Generate Mind Map
response = requests.post(
    f"{BASE_URL}/mindmap",
    data={
        "text": extracted_text,
        "language": "en"
    }
)
mindmap = response.json()["mindmap"]
print("Mind Map:", mindmap)

# 6. Chat with Document
response = requests.post(
    f"{BASE_URL}/chat",
    data={
        "filename": "document.pdf",
        "user_message": "What are the main topics?",
        "context": extracted_text,
        "language": "en"
    }
)
answer = response.json()["answer"]
print("Answer:", answer)

# 7. Generate Everything
response = requests.post(
    f"{BASE_URL}/generate-all",
    data={
        "text": extracted_text,
        "filename": "document.pdf"
    }
)
all_features = response.json()
print("All Features Generated!")
print("English Summary:", all_features["features"]["en"]["summary"][:200])
print("Arabic Summary:", all_features["features"]["ar"]["summary"][:200])
```

---

## 🔧 Configuration

### Environment Variables

```bash
MISTRAL_API_KEY=your_api_key_here
```

### Uvicorn Settings

```bash
# Development
uvicorn fastapi_app:app --host 0.0.0.0 --port 8000 --reload

# Production
uvicorn fastapi_app:app --host 0.0.0.0 --port 8000 --workers 4
```

---

## 📦 Project Structure

```
study-assistant-api/
├── fastapi_app.py          # Main FastAPI application
├── utils.py                # Utility functions for file processing & AI
├── requirements.txt        # Python dependencies
├── README.md              # This file
└── .env                   # Environment variables (create this)
```

---

## 🔑 Getting Mistral API Key

1. Visit [Mistral AI Console](https://console.mistral.ai/)
2. Sign up or log in
3. Create a new API key
4. Use it in `/initialize` endpoint or set `MISTRAL_API_KEY` environment variable

---

## ⚠️ Error Handling

The API returns standardized error responses:

```json
{
  "error": "Mistral API not initialized",
  "detail": "Call /initialize endpoint first"
}
```

**Common Error Codes:**
- `400`: Bad Request (missing/invalid parameters)
- `500`: Server Error (processing failed)
- `413`: Payload Too Large (file > 50MB)

---

## 🚀 Deployment

### Docker (Optional)

Create `Dockerfile`:
```dockerfile
FROM python:3.10-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install -r requirements.txt

COPY . .

CMD ["uvicorn", "fastapi_app:app", "--host", "0.0.0.0", "--port", "8000"]
```

Run:
```bash
docker build -t study-assistant-api .
docker run -p 8000:8000 -e MISTRAL_API_KEY=your_key study-assistant-api
```

### Heroku, AWS, or DigitalOcean

Deploy the same way you would any FastAPI app!

---

## 📚 Key Features Comparison

| Feature | Description | Language |
|---------|-------------|----------|
| 📄 Summary | Structured key points | EN + AR |
| ❓ Quiz | 4-option MCQ with feedback | EN + AR |
| 🧠 Mind Map | Visual hierarchy | EN + AR |
| 📚 Question Bank | Mixed question types | EN + AR |
| 💬 Chat | Document Q&A | EN + AR |
| 📤 File Upload | Multi-format support | - |
| 🎯 OCR | Image/PDF text extraction | - |

---

## 🙋 Support

- **API Documentation**: http://localhost:8000/docs
- **Mistral API Docs**: https://docs.mistral.ai/

---

## 📄 License

MIT License - Feel free to use in your projects!

---

## 🎯 Future Enhancements

- [ ] Database support (PostgreSQL)
- [ ] User authentication & history
- [ ] File storage & management
- [ ] Advanced caching
- [ ] Rate limiting
- [ ] Analytics & metrics
- [ ] Multiple AI models support
- [ ] WebSocket for real-time chat

---

**Happy Learning! 🎓**
