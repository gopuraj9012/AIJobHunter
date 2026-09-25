# TailorTalent AI Service
This is a FastAPI-based AI service that provides resume parsing, job analysis, tailoring suggestions, and cover letter generation.

## Features
- `POST /parse-resume`: Converts unstructured resume text into a structured JSON format.
- `POST /analyze-job`: Extracts keywords and requirements from a job description.
- `POST /tailor-resume`: Analyzes a resume against a job description and provides suggestions.
- `POST /generate-cover-letter`: Generates a tailored cover letter.

## Environment Variables
- `OPENAI_API_KEY`: Required for live LLM-based analysis. If not provided, the service falls back to high-quality mock data for development.
- `AI_MODEL`: (Optional) The OpenAI model to use (default: `gpt-4o`).

## Running the Service
1. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```
2. Start the service:
   ```bash
   python3 main.py
   ```
   The service will be available at `http://localhost:8000`.

## Running the Tests
```bash
pip install -r requirements-dev.txt
python -m pytest tests/ -v
```
The contract tests exercise `POST /parse-resume` against the FastAPI app in-process:
- 200 + full `ResumeData` schema (snake_case fields: `personal_info`, `start_date`, `end_date`, `graduation_date`, ...) when a valid `resume_text` is sent (mock mode without `OPENAI_API_KEY`).
- 422 when `resume_text` is missing or empty.
- The response schema is the same one the .NET backend (`AiIntegrationService.ParseResumeAsync` → `POST /parse-resume` with `{"resume_text": ...}`) deserializes into `ResumeData`.

## Service-Down Behavior
The .NET backend interacts with this service via `HttpClient`. If the AI service is down:
- The backend will receive a `HttpRequestException`.
- The current implementation in `AiIntegrationService` calls `EnsureSuccessStatusCode()`, which will throw an exception.
- It is recommended that the UI handles these errors gracefully by showing a "Service temporarily unavailable" message to the user, rather than a raw error.
- For local development, the service must be running for these features to function.

## Integrated Flow Testing
To verify the parsing endpoint:
```bash
curl -X POST http://localhost:8000/parse-resume \
     -H "Content-Type: application/json" \
     -d '{"resume_text": "Your Resume Text Here"}'
```
Valid responses match the `ResumeData` schema defined in `app/models.py`.