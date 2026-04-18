"""
FastAPI Study Assistant Server
AI-powered study assistant with document processing and multiple learning features

Copyright (c) Ahmed Mohamed Abdel Fattah
"""


from fastapi.responses import StreamingResponse
from fastapi.responses import StreamingResponse, FileResponse, JSONResponse
from fastapi import FastAPI, UploadFile, File, HTTPException, Form
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse, JSONResponse
from pydantic import BaseModel
from typing import Optional, List, Dict, Tuple
import os
import base64
import json
import re
from io import BytesIO
from datetime import datetime
import hashlib
import uvicorn
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

from mistralai import Mistral
from docx import Document as DocxDocument
from pptx import Presentation

# LangChain replaced with built-in splitter for Python 3.14 compatibility
def RecursiveCharacterTextSplitter(chunk_size=1000, chunk_overlap=200):
    class Splitter:
        def __init__(self, chunk_size, chunk_overlap):
            self.chunk_size = chunk_size
            self.chunk_overlap = chunk_overlap
        def split_text(self, text):
            chunks = []
            start = 0
            while start < len(text):
                end = start + self.chunk_size
                chunks.append(text[start:end])
                start += self.chunk_size - self.chunk_overlap
            return chunks
    return Splitter(chunk_size, chunk_overlap)

# Import utility functions
from utils import (
    extract_from_word,
    extract_from_txt,
    pptx_extract_text,
    ocr_mistral,
    clean_text,
    chunk_text,
    get_file_extension,
    generate_summary,
    generate_quiz,
    generate_mindmap,
    generate_question_bank,
    generate_chat_response,
    validate_file,
    get_text_hash,
)

# ===============================
# FASTAPI APP CONFIG
# ===============================
app = FastAPI(
    title="AI Study Assistant API",
    description="FastAPI backend for AI-powered study materials generation",
    version="1.0.0"
)

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ===============================
# PYDANTIC MODELS (Request/Response)
# ===============================
class TextProcessRequest(BaseModel):
    text: str
    language: str = "en"

class ChatMessage(BaseModel):
    role: str
    content: str
    timestamp: Optional[str] = None

class ChatRequest(BaseModel):
    filename: str
    user_message: str
    context: str
    language: str = "en"

class QuizResponse(BaseModel):
    questions: List[Dict]
    error: Optional[str] = None

class FileExtractionResponse(BaseModel):
    filename: str
    text: str
    metadata: Dict
    word_count: int
    character_count: int
    chunks_count: int

class AllFeaturesResponse(BaseModel):
    filename: str
    text_hash: str
    language: str
    features: Dict
    generated_at: str

# ===============================
# GLOBAL MISTRAL CLIENT
# ===============================
mistral_client = None

def initialize_mistral(api_key: str):
    global mistral_client
    try:
        mistral_client = Mistral(api_key=api_key)
        return True
    except Exception as e:
        print(f"❌ Failed to initialize Mistral: {e}")
        return False

# Auto-initialize from environment variable on startup
MISTRAL_API_KEY = os.getenv("MISTRAL_API_KEY", "").strip()
if MISTRAL_API_KEY:
    print(f"🔑 Auto-initializing Mistral API from environment variable...")
    if initialize_mistral(MISTRAL_API_KEY):
        print("✅ Mistral API initialized successfully from .env file!")
    else:
        print("⚠️ Failed to initialize Mistral API - check your API key")
else:
    print("⚠️ MISTRAL_API_KEY not found in environment. Run /initialize endpoint manually.")


# ===============================
# ENDPOINTS
# ===============================

@app.get("/")
async def root():
    return {
        "message": "Welcome to AI Study Assistant API",
        "version": "1.0.0",
        "endpoints": {
            "initialize": "POST /initialize (set API key)",
            "health": "GET /health",
            "extract_text": "POST /extract",
            "generate_summary": "POST /summary",
            "generate_quiz": "POST /quiz",
            "generate_mindmap": "POST /mindmap",
            "generate_qbank": "POST /question-bank",
            "generate_all": "POST /generate-all",
            "chat": "POST /chat",
        }
    }

@app.get("/health")
async def health_check():
    return {
        "status": "ok",
        "mistral_initialized": mistral_client is not None,
        "timestamp": datetime.now().isoformat()
    }

@app.post("/initialize")
async def initialize(api_key: str = Form(...)):
    if initialize_mistral(api_key):
        return {"status": "success", "message": "Mistral client initialized"}
    else:
        raise HTTPException(status_code=400, detail="Failed to initialize Mistral client")

@app.post("/extract", response_model=FileExtractionResponse)
async def extract_text(file: UploadFile = File(...)):
    if not mistral_client:
        raise HTTPException(status_code=400, detail="Mistral API not initialized. Call /initialize first")

    file_content = await file.read()
    is_valid, validation_msg = validate_file(file_content, file.filename)
    if not is_valid:
        raise HTTPException(status_code=400, detail=validation_msg)

    filename = file.filename
    ext = get_file_extension(filename)

    try:
        await file.seek(0)

        if ext == ".docx":
            extracted_text, metadata = extract_from_word(file.file)
        elif ext == ".txt":
            extracted_text, metadata = extract_from_txt(file.file)
        elif ext == ".pptx":
            extracted_text, metadata = pptx_extract_text(file.file, mistral_client)
        elif ext in [".pdf", ".png", ".jpg", ".jpeg"]:
            extracted_text, metadata = ocr_mistral(file.file, filename, mistral_client)
        else:
            raise HTTPException(status_code=400, detail=f"Unsupported file type: {ext}")

        if extracted_text.startswith("Error") or extracted_text.startswith("❌"):
            raise HTTPException(status_code=400, detail=extracted_text)

        word_count = len(extracted_text.split())
        char_count = len(extracted_text)
        chunks = chunk_text(extracted_text)

        return FileExtractionResponse(
            filename=filename,
            text=extracted_text,
            metadata=metadata,
            word_count=word_count,
            character_count=char_count,
            chunks_count=len(chunks)
        )

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error processing file: {str(e)}")

@app.post("/summary")
async def generate_summary_endpoint(
    text: str = Form(...),
    language: str = Form("en")
):
    if not mistral_client:
        raise HTTPException(status_code=400, detail="Mistral API not initialized")
    if not text or not text.strip():
        raise HTTPException(status_code=400, detail="Text cannot be empty")
    if language not in ["en", "ar"]:
        raise HTTPException(status_code=400, detail="Language must be 'en' or 'ar'")

    try:
        summary = generate_summary(mistral_client, text, language)
        return {
            "summary": summary,
            "language": language,
            "generated_at": datetime.now().isoformat()
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generating summary: {str(e)}")

@app.post("/quiz", response_model=QuizResponse)
async def generate_quiz_endpoint(
    text: str = Form(...),
    num_questions: int = Form(10),
    language: str = Form("en")
):
    if not mistral_client:
        raise HTTPException(status_code=400, detail="Mistral API not initialized")
    if language not in ["en", "ar"]:
        raise HTTPException(status_code=400, detail="Language must be 'en' or 'ar'")
    if num_questions < 1 or num_questions > 50:
        raise HTTPException(status_code=400, detail="Number of questions must be between 1 and 50")

    try:
        quiz = generate_quiz(mistral_client, text, num_questions, language)
        return quiz
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generating quiz: {str(e)}")

@app.post("/mindmap")
async def generate_mindmap_endpoint(
    text: str = Form(...),
    language: str = Form("en")
):
    if not mistral_client:
        raise HTTPException(status_code=400, detail="Mistral API not initialized")
    if language not in ["en", "ar"]:
        raise HTTPException(status_code=400, detail="Language must be 'en' or 'ar'")

    try:
        mindmap = generate_mindmap(mistral_client, text, language)
        return {
            "mindmap": mindmap,
            "language": language,
            "generated_at": datetime.now().isoformat()
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generating mind map: {str(e)}")

@app.post("/question-bank")
async def generate_qbank_endpoint(
    text: str = Form(...),
    num_questions: int = Form(20),
    language: str = Form("en")
):
    if not mistral_client:
        raise HTTPException(status_code=400, detail="Mistral API not initialized")
    if language not in ["en", "ar"]:
        raise HTTPException(status_code=400, detail="Language must be 'en' or 'ar'")
    if num_questions < 5 or num_questions > 100:
        raise HTTPException(status_code=400, detail="Number of questions must be between 5 and 100")

    try:
        qbank = generate_question_bank(mistral_client, text, num_questions, language)
        return {
            "question_bank": qbank,
            "num_questions": num_questions,
            "language": language,
            "generated_at": datetime.now().isoformat()
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generating question bank: {str(e)}")

@app.post("/generate-all", response_model=AllFeaturesResponse)
async def generate_all_features(
    file: UploadFile = File(...),
    language: str = Form("en"),
    num_quiz_questions: int = Form(10),
    num_bank_questions: int = Form(20),
):
    """
    Upload a file and generate ALL features (Summary, Quiz, Mind Map, Question Bank)
    in the selected language.

    **Parameters:**
    - file: The document (PDF, Word, PowerPoint, Image, TXT)
    - language: 'en' for English or 'ar' for Arabic (default: 'en')
    - num_quiz_questions: Number of quiz questions (default: 10)
    - num_bank_questions: Number of question bank questions (default: 20)
    """
    if not mistral_client:
        raise HTTPException(status_code=400, detail="Mistral API not initialized")

    if language not in ["en", "ar"]:
        raise HTTPException(status_code=400, detail="Language must be 'en' or 'ar'")

    # ── Step 1: Extract text from file ──
    file_content = await file.read()
    is_valid, validation_msg = validate_file(file_content, file.filename)
    if not is_valid:
        raise HTTPException(status_code=400, detail=validation_msg)

    filename = file.filename
    ext = get_file_extension(filename)

    try:
        await file.seek(0)

        if ext == ".docx":
            extracted_text, metadata = extract_from_word(file.file)
        elif ext == ".txt":
            extracted_text, metadata = extract_from_txt(file.file)
        elif ext == ".pptx":
            extracted_text, metadata = pptx_extract_text(file.file, mistral_client)
        elif ext in [".pdf", ".png", ".jpg", ".jpeg"]:
            extracted_text, metadata = ocr_mistral(file.file, filename, mistral_client)
        else:
            raise HTTPException(status_code=400, detail=f"Unsupported file type: {ext}")

        if extracted_text.startswith("Error") or extracted_text.startswith("❌"):
            raise HTTPException(status_code=400, detail=extracted_text)

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error extracting file: {str(e)}")

    # ── Step 2: Generate all features in chosen language ──
    try:
        text_hash = get_text_hash(extracted_text)

        features = {
            "summary":       generate_summary(mistral_client, extracted_text, language),
            "quiz":          generate_quiz(mistral_client, extracted_text, num_quiz_questions, language),
            "mindmap":       generate_mindmap(mistral_client, extracted_text, language),
            "question_bank": generate_question_bank(mistral_client, extracted_text, num_bank_questions, language),
        }

        return AllFeaturesResponse(
            filename=filename,
            text_hash=text_hash,
            language=language,
            features=features,
            generated_at=datetime.now().isoformat()
        )

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generating features: {str(e)}")

@app.post("/chat")
async def chat_endpoint(request: ChatRequest):
    if not mistral_client:
        raise HTTPException(status_code=400, detail="Mistral API not initialized")
    if not request.user_message or not request.user_message.strip():
        raise HTTPException(status_code=400, detail="Message cannot be empty")

    # Detect language: ar / eg / or anything containing Arabic keywords
    lang = request.language.strip().lower() if request.language else "en"
    is_arabic = lang in ["ar", "eg"] or any(w in lang for w in ["arabic", "egyptian", "عربي", "عامية"])
    is_egyptian = lang == "eg" or "egyptian" in lang or "عامية" in lang

    if is_egyptian:
        system_prompt = f"""أنت مساعد دراسة بتساعد الطالب يفهم المستند اللي رفعه.

محتوى المستند:
{request.context[:8000]}

تعليمات مهمة جداً:
1. لازم تتكلم بالعامية المصرية في كل ردودك
2. لو الطالب بيتكلم بالعامية، ارد بالعامية
3. لو بيتكلم بالعربي الفصيح، ارد بالعامية المصرية
4. لو بيتكلم بالإنجليزي، ارد بالعامية المصرية
5. جاوب بس من المستند، لو السؤال برا المستند قوله بأدب
6. كن مفيد وواضح وبسيط في الشرح"""
    elif is_arabic:
        system_prompt = f"""أنت مساعد دراسة متخصص في شرح ومناقشة المستند المرفوع.

محتوى المستند:
{request.context[:8000]}

تعليمات مهمة:
1. أجب دائماً باللغة العربية الفصحى
2. افهم الأسئلة سواء كانت بالعربية الفصحى أو العامية المصرية
3. أجب فقط بناءً على محتوى المستند
4. إذا كان السؤال خارج نطاق المستند، قل ذلك بأدب
5. كن مفيداً وتعليمياً ودقيقاً"""
    else:
        system_prompt = f"""You are an AI Study Assistant for the uploaded document.

DOCUMENT CONTEXT:
{request.context[:8000]}

INSTRUCTIONS:
1. Answer ONLY based on the document content
2. If outside scope, politely say so
3. Answer in English
4. Be helpful, educational, and precise"""

    async def stream_response():
        try:
            with mistral_client.chat.stream(
                model="mistral-large-latest",
                messages=[
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": request.user_message}
                ],
                temperature=0.7,
                max_tokens=1500,
            ) as stream:
                for chunk in stream:
                    delta = chunk.data.choices[0].delta.content
                    if delta:
                        yield delta
        except Exception as e:
            yield f"\n❌ Error: {str(e)}"

    return StreamingResponse(
        stream_response(),
        media_type="text/plain",
        headers={
            "X-Question": request.user_message,
            "X-Language": request.language,
            "X-Filename": request.filename,
            "X-Timestamp": datetime.now().isoformat(),
        }
    )
@app.post("/batch-chat")
async def batch_chat_endpoint(
    filename: str = Form(...),
    context: str = Form(...),
    messages: str = Form(...)
):
    if not mistral_client:
        raise HTTPException(status_code=400, detail="Mistral API not initialized")

    try:
        message_list = json.loads(messages)
        if not isinstance(message_list, list):
            raise HTTPException(status_code=400, detail="Messages must be a JSON array")

        responses = []
        for msg in message_list:
            if 'content' not in msg:
                continue
            response = generate_chat_response(
                mistral_client,
                context,
                msg['content'],
                msg.get('language', 'en')
            )
            responses.append({
                "question": msg['content'],
                "answer": response,
                "language": msg.get('language', 'en')
            })

        return {
            "filename": filename,
            "responses": responses,
            "total_processed": len(responses),
            "timestamp": datetime.now().isoformat()
        }

    except json.JSONDecodeError:
        raise HTTPException(status_code=400, detail="Invalid JSON in messages")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error processing batch: {str(e)}")

# ===============================
# ERROR HANDLERS
# ===============================
@app.exception_handler(HTTPException)
async def http_exception_handler(request, exc):
    return JSONResponse(
        status_code=exc.status_code,
        content={"error": exc.detail}
    )

@app.exception_handler(Exception)
async def general_exception_handler(request, exc):
    return JSONResponse(
        status_code=500,
        content={"error": "Internal server error", "detail": str(exc)}
    )

# ===============================
# RUN SERVER
# ===============================
if __name__ == "__main__":
    print("""
    ╔╗ ╔═╗╔═╗╔═╗  ╔═╗╔═╗╔╦╗╦ ╦╔╦╗╦ ╦
    ╠╩╗╠═╣╚═╗║╣   ╚═╗╠╦╝║║║║ ║ ║║╩╫╫
    ╚═╝╩ ╩╚═╝╚═╝  ╚═╝╩╚═╩ ╩╚═╩═╝╩ ╩╩ ╩

    AI Study Assistant - FastAPI Server

    Starting server on http://localhost:8000
    API Docs: http://localhost:8000/docs
    """)

    uvicorn.run(
        "fastapi_app:app",
        host="0.0.0.0",
        port=8000,
        reload=True,
        log_level="info"
    )