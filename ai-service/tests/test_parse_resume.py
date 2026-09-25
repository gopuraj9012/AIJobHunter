"""Contract tests for POST /parse-resume.

These assert the exact request/response contract the .NET backend depends on
(see TailorTalent.Api AiIntegrationService.ParseResumeAsync, which POSTs
{"resume_text": ...} and deserializes the snake_case ResumeData response).

Run from ai-service/:  python -m pytest tests/ -v
Requires: pip install -r requirements-dev.txt  (fastapi, uvicorn, pydantic, pytest, httpx)
"""
import os

# Force the deterministic mock branch: ai_service reads OPENAI_API_KEY at import time.
os.environ.pop("OPENAI_API_KEY", None)

import pytest
from fastapi.testclient import TestClient

from main import app

SAMPLE_RESUME = """John Smith
San Francisco, CA | 555-0199 | john.smith@email.com

Professional Summary
Dedicated Software Engineer with 8 years of experience in full-stack development.

Experience
Senior Software Engineer | Cloud Solutions Inc | Jan 2018 - Present
- Led a team of 5 developers to build a scalable microservices platform.
- Improved system uptime by 15% through robust monitoring.

Education
B.S. in Computer Science | University of California, Berkeley | 2014

Skills
Python, JavaScript, AWS, Docker, PostgreSQL
"""


@pytest.fixture(scope="module")
def client():
    return TestClient(app)


def test_root_health(client):
    """GET / is the health/liveness probe used by ops."""
    resp = client.get("/")
    assert resp.status_code == 200
    body = resp.json()
    assert body["status"] == "ok"
    assert "service" in body


def test_parse_resume_returns_200_with_full_schema(client):
    """Valid resume_text -> 200 and the complete .NET-compatible ResumeData schema."""
    resp = client.post("/parse-resume", json={"resume_text": SAMPLE_RESUME})
    assert resp.status_code == 200
    assert resp.headers["content-type"].startswith("application/json")

    body = resp.json()

    # Top-level contract (keys the .NET ResumeData model binds via snake_case)
    for key in ("personal_info", "summary", "experience", "education", "skills"):
        assert key in body, f"missing top-level key '{key}' in {list(body)}"

    # personal_info
    pi = body["personal_info"]
    assert pi["name"] == "John Doe"  # deterministic mock (no OPENAI_API_KEY)
    assert pi["email"] == "john.doe@example.com"
    for field in ("phone", "location", "linkedin", "website"):
        assert field in pi, f"personal_info missing '{field}'"

    # experience list items carry snake_case date fields used by the builder
    assert isinstance(body["experience"], list) and len(body["experience"]) >= 1
    exp0 = body["experience"][0]
    for field in ("company", "title", "location", "start_date", "end_date", "description", "highlights"):
        assert field in exp0, f"experience[0] missing '{field}'"
    assert isinstance(exp0["highlights"], list)

    # education list items carry graduation_date
    assert isinstance(body["education"], list) and len(body["education"]) >= 1
    edu0 = body["education"][0]
    for field in ("school", "degree", "location", "graduation_date", "description"):
        assert field in edu0, f"education[0] missing '{field}'"

    # skills is a list of strings
    assert isinstance(body["skills"], list)
    assert all(isinstance(s, str) for s in body["skills"])


def test_parse_resume_requires_resume_text(client):
    """Missing resume_text -> 422 (FastAPI validation)."""
    resp = client.post("/parse-resume", json={})
    assert resp.status_code == 422


def test_parse_resume_rejects_empty_resume_text(client):
    """Empty resume_text -> 422 (min_length=1 guard)."""
    resp = client.post("/parse-resume", json={"resume_text": ""})
    assert resp.status_code == 422


def test_parse_resume_is_json_body_only(client):
    """The endpoint must not bind raw query text; it needs a JSON body."""
    resp = client.post("/parse-resume", data=SAMPLE_RESUME)  # text/plain, no JSON
    assert resp.status_code == 422