#!/usr/bin/env python
"""
AI Study Assistant API - Auto-Start Script
Run this to start the server with your API key (no manual initialization needed)
"""

import os
import sys
import subprocess
from pathlib import Path

def start_server(api_key: str):
    """Start the FastAPI server with the provided API key"""
    
    # Get the directory where this script is located
    project_dir = Path(__file__).parent.absolute()
    
    # Create/update .env file with the API key
    env_file = project_dir / ".env"
    
    print("\n" + "="*70)
    print("🎓 AI Study Assistant - FastAPI Server")
    print("="*70 + "\n")
    
    print(f"📁 Project directory: {project_dir}")
    print(f"🔑 Saving API key to .env file...")
    
    # Write .env file
    env_content = f"""# Mistral AI Configuration
MISTRAL_API_KEY={api_key}

# API Server Configuration
API_HOST=0.0.0.0
API_PORT=8000
API_RELOAD=True
LOG_LEVEL=info

# File Processing
MAX_FILE_SIZE_MB=50
CHUNK_SIZE=500
CHUNK_OVERLAP=100

# Feature Flags
ENABLE_PDF_EXPORT=true
ENABLE_ARABIC_SUPPORT=true
ENABLE_BATCHING=true
"""
    
    with open(env_file, "w") as f:
        f.write(env_content)
    
    print(f"✅ API key saved to .env file\n")
    
    # Start the server
    print("🚀 Starting FastAPI server...")
    print("-" * 70)
    print("\n📍 Server will be available at:")
    print("   🌐 API Docs:     http://localhost:8000/docs")
    print("   📚 Alt Docs:     http://localhost:8000/redoc")
    print("   💚 Health:       http://localhost:8000/health")
    print("\n✨ Auto-initialized with your Mistral API key!")
    print("✨ Ready to extract files, generate summaries, quizzes, and more!\n")
    print("-" * 70)
    print("\n Press Ctrl+C to stop the server\n")
    
    # Change to project directory and run uvicorn
    os.chdir(project_dir)
    
    # Run Uvicorn
    subprocess.run([
        sys.executable, "-m", "uvicorn",
        "fastapi_app:app",
        "--host", "0.0.0.0",
        "--port", "8000",
        "--reload"
    ])

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("\n" + "="*70)
        print("❌ Error: API key not provided!")
        print("="*70)
        print("\nUsage:")
        print("  python start_server.py YOUR_MISTRAL_API_KEY")
        print("\nExample:")
        print("  python start_server.py sk-abcd1234efgh5678...")
        print("\n📍 Get your free API key at: https://console.mistral.ai/")
        print("="*70 + "\n")
        sys.exit(1)
    
    api_key = sys.argv[1]
    
    if not api_key or len(api_key) < 10:
        print("\n❌ Error: Invalid API key (too short)")
        sys.exit(1)
    
    try:
        start_server(api_key)
    except KeyboardInterrupt:
        print("\n\n⛔ Server stopped by user")
        sys.exit(0)
    except Exception as e:
        print(f"\n❌ Error: {e}")
        sys.exit(1)
