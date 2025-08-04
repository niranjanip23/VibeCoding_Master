# QueryHub End-to-End Testing Guide

## Overview
This guide provides comprehensive end-to-end testing for the QueryHub application, covering complete user flows from frontend to backend.

## Test Categories

### 1. Authentication Flow Tests
- User Registration (Frontend → Backend → Database)
- User Login (Frontend → Backend → JWT Token)
- Protected Route Access
- Logout Flow

### 2. Question Management Tests  
- Create Question (Frontend → Backend → Database)
- View Questions List (Backend → Frontend Display)
- View Question Details (Backend → Frontend Display)
- Search Questions
- Filter by Tags

### 3. Answer Management Tests
- Create Answer (Frontend → Backend → Database)
- View Answers on Question Details
- Answer Validation

### 4. Voting System Tests
- Vote on Questions
- Vote on Answers
- Vote Count Updates

### 5. Data Flow Tests
- Database Persistence
- API Response Validation
- Frontend Data Display
- Error Handling

## Prerequisites
1. Backend running on http://localhost:5031
2. Frontend running on http://localhost:5000
3. Database initialized with sample data
4. PowerShell execution policy allows scripts

## Test Scripts Available
- `test-e2e-auth.ps1` - Authentication flow testing
- `test-e2e-questions.ps1` - Question management testing
- `test-e2e-complete.ps1` - Complete user journey testing
- `test-e2e-manual.ps1` - Manual testing checklist

## Running Tests
```powershell
# Run all E2E tests
.\run-all-e2e-tests.ps1

# Run individual test suites
.\test-e2e-auth.ps1
.\test-e2e-questions.ps1
.\test-e2e-complete.ps1
```

## Expected Outcomes
All tests should pass with green checkmarks, indicating successful frontend-backend integration and data flow.
