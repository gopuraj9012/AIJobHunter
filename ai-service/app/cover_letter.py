import os
import json
from .models import CoverLetterRequest, CoverLetterResponse
from .prompts import COVER_LETTER_PROMPT

class CoverLetterService:
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

    async def generate(self, request: CoverLetterRequest) -> CoverLetterResponse:
        if not self.client:
            # Mock behavior
            content = f"""Dear Hiring Manager,

I am writing to express my strong interest in the role as described in your job description. With my extensive experience in Python and AWS, as highlighted in my resume, I am confident that I can contribute effectively to your team.

I have a proven track record of designing scalable backends and collaborating with cross-functional teams to deliver high-quality software. My experience with cloud architecture and unit testing aligns perfectly with your requirements.

Thank you for your time and consideration.

Best regards,
[Candidate Name]"""
            
            return CoverLetterResponse(
                content=content,
                key_points_addressed=["Python experience", "AWS cloud architecture", "Backend design"],
                tailoring_notes="Focused on the overlap between candidate's 6 years of Python experience and the JD's requirement for a Senior Developer."
            )
        
        prompt_input = f"Resume:\n{request.resume_text}\n\nJob Description:\n{request.job_description}"
        system_prompt = COVER_LETTER_PROMPT.format(tone=request.tone or "professional")
        
        response = self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": prompt_input}
            ],
            response_format={"type": "json_object"}
        )
        data = json.loads(response.choices[0].message.content)
        return CoverLetterResponse(**data)

cover_letter_service = CoverLetterService()
