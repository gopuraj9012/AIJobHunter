import os
import json
import re
from typing import List, Optional
from .models import (
    JobAnalysis, TailoringResult, Suggestion, ScoreBreakdown,
    ResumeData, PersonalInfo,
)
from .prompts import KEYWORD_EXTRACTION_PROMPT, GAP_ANALYSIS_PROMPT, RESUME_PARSE_PROMPT

class AIService:
    def __init__(self):
        self.api_key = os.getenv("OPENAI_API_KEY")
        self.model = os.getenv("AI_MODEL", "gpt-4o")
        self.client = None
        
        if self.api_key:
            try:
                from openai import OpenAI
                self.client = OpenAI(api_key=self.api_key)
            except ImportError:
                print("OpenAI library not installed. Falling back to mock.")

    async def analyze_job(self, jd_text: str) -> JobAnalysis:
        if not self.client:
            # Mock behavior
            return JobAnalysis(
                keywords=["Python", "AWS", "Docker", "REST API", "Microservices"],
                required_skills=["5+ years Python", "Cloud architecture", "Unit testing"],
                preferred_skills=["Kubernetes", "GraphQL"],
                core_responsibilities=["Design scalable backends", "Collaborate with frontend teams"]
            )
        
        response = self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": KEYWORD_EXTRACTION_PROMPT},
                {"role": "user", "content": jd_text}
            ],
            response_format={"type": "json_object"}
        )
        data = json.loads(response.choices[0].message.content)
        return JobAnalysis(**data)

    async def tailor_resume(self, resume_text: str, job_analysis: JobAnalysis) -> TailoringResult:
        if not self.client:
            # Mock behavior
            suggestions = [
                Suggestion(
                    section="Skills",
                    feedback="Add 'Docker' and 'Microservices' to your skills section as they are highly relevant for this role.",
                    suggested_rewrite="Skills: Python, AWS, Docker, Kubernetes, Microservices, REST API"
                ),
                Suggestion(
                    section="Experience",
                    feedback="Quantify your experience with AWS in your latest role.",
                    suggested_rewrite="Optimized cloud infrastructure on AWS, reducing latency by 20% using Microservices architecture."
                )
            ]
            
            return TailoringResult(
                match_score=75,
                match_score_breakdown=ScoreBreakdown(
                    skills=80,
                    experience=70,
                    education=90
                ),
                missing_keywords=["Docker", "Microservices"],
                high_impact_missing_keywords=["Docker", "Microservices", "Kubernetes"],
                strengths=["Strong Python background", "Cloud experience"],
                weaknesses=["Lacks explicit mention of containerization"],
                experience_bullet_suggestions=[
                    "Spearheaded the migration of legacy monolith to a microservices architecture using Docker, improving deployment frequency by 40%.",
                    "Architected and deployed scalable backend services on AWS, leveraging Lambda and S3 to handle 1M+ daily requests.",
                    "Implemented comprehensive unit and integration testing suites, achieving 90% code coverage across all core modules."
                ],
                improvement_suggestions=suggestions
            )
        
        prompt_input = f"Resume:\n{resume_text}\n\nJob Description Analysis:\n{job_analysis.model_dump_json()}"
        
        response = self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": GAP_ANALYSIS_PROMPT},
                {"role": "user", "content": prompt_input}
            ],
            response_format={"type": "json_object"}
        )
        data = json.loads(response.choices[0].message.content)
        return TailoringResult(**data)

    async def parse_resume(self, resume_text: str) -> ResumeData:
        if not self.client:
            # Mock behavior: cheap heuristic extraction so the pipeline works offline
            email_match = re.search(r"[\w.+-]+@[\w-]+\.[\w.-]+", resume_text)
            phone_match = re.search(r"(\+?\d[\d\s().-]{7,}\d)", resume_text)
            first_line = next(
                (line.strip() for line in resume_text.splitlines() if line.strip()), None
            )
            return ResumeData(
                personal_info=PersonalInfo(
                    name=first_line,
                    email=email_match.group(0) if email_match else None,
                    phone=phone_match.group(1).strip() if phone_match else None,
                ),
                summary=None,
                experience=[],
                education=[],
                skills=[],
            )

        response = self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": RESUME_PARSE_PROMPT},
                {"role": "user", "content": resume_text}
            ],
            response_format={"type": "json_object"}
        )
        data = json.loads(response.choices[0].message.content)
        return ResumeData(**data)

ai_service = AIService()
