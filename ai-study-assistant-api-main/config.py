"""
Configuration file for API
"""

import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

# ===============================
# API Configuration
# ===============================
API_PORT = int(os.getenv("API_PORT", 8000))
API_HOST = os.getenv("API_HOST", "0.0.0.0")
API_RELOAD = os.getenv("API_RELOAD", "True").lower() == "true"

# ===============================
# Mistral Configuration
# ===============================
MISTRAL_API_KEY = os.getenv("MISTRAL_API_KEY", "")
MISTRAL_MODEL = os.getenv("MISTRAL_MODEL", "mistral-large-latest")
MISTRAL_OCR_MODEL = os.getenv("MISTRAL_OCR_MODEL", "mistral-ocr-latest")

# ===============================
# File Processing Configuration
# ===============================
MAX_FILE_SIZE_MB = int(os.getenv("MAX_FILE_SIZE_MB", 50))
CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", 500))
CHUNK_OVERLAP = int(os.getenv("CHUNK_OVERLAP", 100))

# ===============================
# AI Generation Configuration
# ===============================
TEMPERATURE_SUMMARY = float(os.getenv("TEMPERATURE_SUMMARY", 0.3))
TEMPERATURE_QUIZ = float(os.getenv("TEMPERATURE_QUIZ", 0.5))
TEMPERATURE_MINDMAP = float(os.getenv("TEMPERATURE_MINDMAP", 0.4))
TEMPERATURE_CHAT = float(os.getenv("TEMPERATURE_CHAT", 0.7))

# ===============================
# Default Question Counts
# ===============================
DEFAULT_QUIZ_QUESTIONS = int(os.getenv("DEFAULT_QUIZ_QUESTIONS", 10))
DEFAULT_QBANK_QUESTIONS = int(os.getenv("DEFAULT_QBANK_QUESTIONS", 20))

# ===============================
# CORS Configuration
# ===============================
CORS_ORIGINS = os.getenv("CORS_ORIGINS", "*").split(",")
CORS_ALLOW_CREDENTIALS = os.getenv("CORS_ALLOW_CREDENTIALS", "true").lower() == "true"
CORS_ALLOW_METHODS = ["*"]
CORS_ALLOW_HEADERS = ["*"]

# ===============================
# Logging Configuration
# ===============================
LOG_LEVEL = os.getenv("LOG_LEVEL", "info").lower()

# ===============================
# Feature Flags
# ===============================
ENABLE_PDF_EXPORT = os.getenv("ENABLE_PDF_EXPORT", "true").lower() == "true"
ENABLE_ARABIC_SUPPORT = os.getenv("ENABLE_ARABIC_SUPPORT", "true").lower() == "true"
ENABLE_BATCHING = os.getenv("ENABLE_BATCHING", "true").lower() == "true"
