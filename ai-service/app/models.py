from typing import List, Optional
from pydantic import BaseModel, Field

class JobAnalysisRequest(BaseModel):
    jd_text: str

class JobAnalysis(BaseModel):
    keywords: List[str] = Field(..., description="List of ATS-critical terms.")
    required_skills: List[str] = Field(..., description="List of essential skills.")
    preferred_skills: List[str] = Field(..., description="List of nice-to-have skills.")
    core_responsibilities: List[str] = Field(..., description="Summary of the main tasks.")

class Suggestion(BaseModel):
    section: str = Field(..., description="The resume section (e.g., Summary, Experience, Skills).")
    feedback: str = Field(..., description="Specific, actionable advice.")
    suggested_rewrite: Optional[str] = Field(None, description="Optional tailored rewrite of the section.")

class ScoreBreakdown(BaseModel):
    skills: int = Field(..., ge=0, le=100, description="Score for skill alignment.")
    experience: int = Field(..., ge=0, le=100, description="Score for experience relevance.")
    education: int = Field(..., ge=0, le=100, description="Score for educational requirements.")

class TailoringResult(BaseModel):
    match_score: int = Field(..., ge=0, le=100, description="Overall alignment score.")
    match_score_breakdown: ScoreBreakdown = Field(..., description="Alignment score breakdown by category.")
    missing_keywords: List[str] = Field(..., description="Important JD keywords missing from the resume.")
    high_impact_missing_keywords: List[str] = Field(..., description="Top 3-5 most critical keywords missing.")
    strengths: List[str] = Field(..., description="Areas where the resume strongly matches the JD.")
    weaknesses: List[str] = Field(..., description="Specific gaps in experience or skills.")
    experience_bullet_suggestions: List[str] = Field(..., description="3-5 specific bullet point suggestions for the Experience section.")
    improvement_suggestions: List[Suggestion] = Field(..., description="Actionable advice for each section.")

class TailoringRequest(BaseModel):
    resume_text: str
    job_analysis: JobAnalysis

class CoverLetterRequest(BaseModel):
    resume_text: str
    job_description: str
    tone: Optional[str] = "professional"

class CoverLetterResponse(BaseModel):
    content: str
    key_points_addressed: List[str]
    tailoring_notes: str
