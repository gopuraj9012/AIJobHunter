from fastapi import FastAPI, HTTPException
from app.models import (
    JobAnalysis,
    JobAnalysisRequest,
    TailoringResult,
    TailoringRequest,
    CoverLetterRequest,
    CoverLetterResponse,
    ParseResumeRequest,
    ResumeData
)
from app.tailor import ai_service
from app.cover_letter import cover_letter_service

app = FastAPI(title="TailorTalent AI Service")


@app.get("/")
async def root():
    return {"status": "ok", "service": "TailorTalent AI Service"}


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
async def parse_resume(request: ParseResumeRequest):
    try:
        return await ai_service.parse_resume(request.resume_text)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    # Bind to all interfaces to allow external access within the sandbox
    uvicorn.run(app, host="0.0.0.0", port=8000)
