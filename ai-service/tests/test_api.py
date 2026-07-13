"""Unit/integration tests for the AI service running in mock mode (no OPENAI_API_KEY)."""
import os
import sys

import pytest
from fastapi.testclient import TestClient

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

# Force mock mode regardless of the developer's environment
os.environ.pop("OPENAI_API_KEY", None)

from main import app  # noqa: E402

client = TestClient(app)


def test_analyze_job_returns_analysis():
    resp = client.post("/analyze-job", json={"jd_text": "Senior Python developer, AWS required."})
    assert resp.status_code == 200
    body = resp.json()
    assert isinstance(body["keywords"], list) and body["keywords"]
    assert isinstance(body["required_skills"], list)
    assert isinstance(body["preferred_skills"], list)
    assert isinstance(body["core_responsibilities"], list)


def test_analyze_job_missing_body_is_422():
    resp = client.post("/analyze-job", json={})
    assert resp.status_code == 422


def test_tailor_resume_returns_scored_result():
    analysis = {
        "keywords": ["Python"],
        "required_skills": ["Python"],
        "preferred_skills": [],
        "core_responsibilities": ["Build APIs"],
    }
    resp = client.post(
        "/tailor-resume",
        json={"resume_text": "Python dev, 6 years.", "job_analysis": analysis},
    )
    assert resp.status_code == 200
    body = resp.json()
    assert 0 <= body["match_score"] <= 100
    breakdown = body["match_score_breakdown"]
    for key in ("skills", "experience", "education"):
        assert 0 <= breakdown[key] <= 100
    assert isinstance(body["missing_keywords"], list)
    assert isinstance(body["improvement_suggestions"], list)
    for s in body["improvement_suggestions"]:
        assert s["section"]
        assert s["feedback"]


def test_generate_cover_letter_returns_content():
    resp = client.post(
        "/generate-cover-letter",
        json={
            "resume_text": "Python dev, 6 years.",
            "job_description": "Senior Python role.",
            "tone": "professional",
        },
    )
    assert resp.status_code == 200
    body = resp.json()
    assert body["content"].strip()
    assert isinstance(body["key_points_addressed"], list)
    assert body["tailoring_notes"]


def test_cover_letter_tone_defaults_to_professional():
    resp = client.post(
        "/generate-cover-letter",
        json={"resume_text": "x", "job_description": "y"},
    )
    assert resp.status_code == 200


def test_parse_resume_extracts_contact_info():
    resume = "Jane Doe\njane.doe@example.com\n+1 (555) 123-4567\nExperienced engineer."
    resp = client.post("/parse-resume", json={"resume_text": resume})
    assert resp.status_code == 200
    body = resp.json()
    info = body["personal_info"]
    assert info["name"] == "Jane Doe"
    assert info["email"] == "jane.doe@example.com"
    assert info["phone"] is not None
    # Mock mode returns empty (not null) collections so the frontend can iterate safely
    assert body["experience"] == []
    assert body["education"] == []
    assert body["skills"] == []


def test_parse_resume_no_contact_info_is_still_200():
    resp = client.post("/parse-resume", json={"resume_text": "just some text"})
    assert resp.status_code == 200
    body = resp.json()
    assert body["personal_info"]["email"] is None


def test_parse_resume_missing_body_is_422():
    resp = client.post("/parse-resume", json={})
    assert resp.status_code == 422
