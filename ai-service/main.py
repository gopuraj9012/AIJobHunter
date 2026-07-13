import os

from dotenv import load_dotenv

# Load .env before app.* imports: AIService/CoverLetterService read env vars at import time
load_dotenv()

from fastapi import FastAPI, HTTPException
from app.models import (
    JobAnalysis,
    JobAnalysisRequest,
    TailoringResult,
    TailoringRequest,
    CoverLetterRequest,
    CoverLetterResponse,
    ResumeParseRequest,
    ResumeData
)
from app.tailor import ai_service
from app.cover_letter import cover_letter_service

app = FastAPI(title="TailorTalent AI Service")

@app.post("/analyze-job", response_model=JobAnalysis)
async def analyze_job(request: JobAnalysisRequest):
    try:
        return await ai_service.analyze_job(request.jd_text)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/tailor-resume", response_model=TailoringResult)
async def tailor_resume(request: TailoringRequest):
    try:
        return await ai_service.tailor_resume(request.resume_text, request.job_analysis)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/generate-cover-letter", response_model=CoverLetterResponse)
async def generate_cover_letter(request: CoverLetterRequest):
    try:
        return await cover_letter_service.generate(request)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/parse-resume", response_model=ResumeData)
async def parse_resume(request: ResumeParseRequest):
    try:
        return await ai_service.parse_resume(request.resume_text)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    host = os.getenv("AI_SERVICE_HOST", "0.0.0.0")
    port = int(os.getenv("AI_SERVICE_PORT", "8000"))
    uvicorn.run(app, host=host, port=port)
