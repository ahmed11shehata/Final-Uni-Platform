# Quick Reference - AYA University IS Backend API

## Base URL
```
http://localhost:8000/api
```

## Authentication
All requests (except login/register) require:
```
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

## Response Format
```json
{
  "error": "Error message",
  "code": "ERROR_CODE"
}
```

---

## 🔐 Authentication Endpoints

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "student@uni.edu",
  "password": "Password123!"
}

Response:
{
  "token": "eyJhbGc...",
  "user": {
    "id": "CS2024001",
    "name": "Ahmed Mohammed",
    "email": "student@uni.edu",
    "role": "student",
    "department": "Computer Science",
    ...
  }
}
```

### Register
```http
POST /api/auth/register
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "name": "New Student",
  "email": "new@uni.edu",
  "password": "Password123!",
  "role": "student",
  "studentId": "CS2024005"
}
```

### Logout
```http
POST /api/auth/logout
Authorization: Bearer <TOKEN>
```

### Refresh Token
```http
POST /api/auth/refresh
Authorization: Bearer <TOKEN>
```

---

## 👨‍🎓 Student Endpoints

### Get Profile
```http
GET /api/student/profile
Authorization: Bearer <STUDENT_TOKEN>
```

### Update Profile
```http
PUT /api/student/profile
Authorization: Bearer <STUDENT_TOKEN>
Content-Type: application/json

{
  "phone": "+20 1001234567",
  "address": "Cairo, Egypt",
  "dob": "2003-05-15"
}
```

### Get Courses
```http
GET /api/student/courses
Authorization: Bearer <STUDENT_TOKEN>
```

### Get Course Details
```http
GET /api/student/courses/{courseId}
Authorization: Bearer <STUDENT_TOKEN>
```

### Get Transcript
```http
GET /api/student/transcript?year=3&semester=fall
Authorization: Bearer <STUDENT_TOKEN>
```

### Get Academic Summary
```http
GET /api/student/academic-summary
Authorization: Bearer <STUDENT_TOKEN>
```

### Get Schedule
```http
GET /api/student/schedule
Authorization: Bearer <STUDENT_TOKEN>
```

### Available Courses for Registration
```http
GET /api/student/registration/available-courses
Authorization: Bearer <STUDENT_TOKEN>
```

### Register for Course
```http
POST /api/student/registration
Authorization: Bearer <STUDENT_TOKEN>
Content-Type: application/json

{
  "courseCode": "CS301"
}
```

### Drop Course
```http
DELETE /api/student/registration/CS301
Authorization: Bearer <STUDENT_TOKEN>
```

### Get Registration Status
```http
GET /api/student/registration/status
Authorization: Bearer <STUDENT_TOKEN>
```

### Get All Quizzes
```http
GET /api/student/quizzes
Authorization: Bearer <STUDENT_TOKEN>
```

### Get Quiz Details
```http
GET /api/student/courses/{courseId}/quizzes/{quizId}
Authorization: Bearer <STUDENT_TOKEN>
```

### Submit Quiz
```http
POST /api/student/quizzes/{quizId}/submit
Authorization: Bearer <STUDENT_TOKEN>
Content-Type: application/json

{
  "answers": [0, 2, 1, 3]
}
```

### Submit Assignment
```http
POST /api/student/assignments/{assignmentId}/submit
Authorization: Bearer <STUDENT_TOKEN>
Content-Type: multipart/form-data

[file binary data]
```

### Get Course Materials
```http
GET /api/student/courses/{courseId}/materials
Authorization: Bearer <STUDENT_TOKEN>
```

### Get Timetable
```http
GET /api/student/timetable
Authorization: Bearer <STUDENT_TOKEN>
```

---

## 👨‍🏫 Instructor Endpoints

### Dashboard
```http
GET /api/instructor/dashboard
Authorization: Bearer <INSTRUCTOR_TOKEN>
```

### Get Courses
```http
GET /api/instructor/courses
Authorization: Bearer <INSTRUCTOR_TOKEN>
```

### Get Course Students
```http
GET /api/instructor/courses/{courseId}/students
Authorization: Bearer <INSTRUCTOR_TOKEN>
```

### Get Assignments
```http
GET /api/instructor/assignments?courseId={courseId}
Authorization: Bearer <INSTRUCTOR_TOKEN>
```

### Create Assignment
```http
POST /api/instructor/assignments
Authorization: Bearer <INSTRUCTOR_TOKEN>
Content-Type: application/json

{
  "title": "Assignment 1",
  "description": "Submit your solution",
  "courseCode": "CS301",
  "deadline": "2025-02-15",
  "maxGrade": 20,
  "allowedFormats": ["pdf", "zip"]
}
```

### Get Submissions
```http
GET /api/instructor/submissions?courseId={courseId}
Authorization: Bearer <INSTRUCTOR_TOKEN>
```

### Grade Submission
```http
POST /api/instructor/submissions/{submissionId}/grade
Authorization: Bearer <INSTRUCTOR_TOKEN>
Content-Type: application/json

{
  "grade": 18,
  "feedback": "Well done!"
}
```

### Approve/Reject Submission
```http
POST /api/instructor/submissions/{submissionId}/approve
Authorization: Bearer <INSTRUCTOR_TOKEN>
Content-Type: application/json

{
  "grade": 18
}
```

```http
POST /api/instructor/submissions/{submissionId}/reject
Authorization: Bearer <INSTRUCTOR_TOKEN>
Content-Type: application/json

{
  "reason": "Plagiarism detected"
}
```

### Get Quizzes
```http
GET /api/instructor/quizzes?courseId={courseId}
Authorization: Bearer <INSTRUCTOR_TOKEN>
```

### Create Quiz
```http
POST /api/instructor/quizzes
Authorization: Bearer <INSTRUCTOR_TOKEN>
Content-Type: application/json

{
  "courseId": "CS301",
  "title": "Quiz 1",
  "duration": 30,
  "gradePerQ": 1,
  "deadline": "2025-02-20T10:00:00",
  "status": "published",
  "questions": [
    {
      "text": "What is X?",
      "answers": [
        { "text": "Option A" },
        { "text": "Option B" },
        { "text": "Option C" },
        { "text": "Option D" }
      ],
      "correct": 2
    }
  ]
}
```

### Get Exam Grades
```http
GET /api/instructor/courses/{courseId}/exam-grades?examType=midterm
Authorization: Bearer <INSTRUCTOR_TOKEN>
```

### Record Exam Grade
```http
POST /api/instructor/courses/{courseId}/exam-grades
Authorization: Bearer <INSTRUCTOR_TOKEN>
Content-Type: application/json

{
  "examType": "midterm",
  "studentId": "CS2024001",
  "grade": 85
}
```

### Upload Material
```http
POST /api/instructor/courses/{courseId}/materials
Authorization: Bearer <INSTRUCTOR_TOKEN>
Content-Type: multipart/form-data

[file binary data]
title=Lecture 1
type=video
description=Week 1 lecture
```

### Get Schedule
```http
GET /api/instructor/schedule
Authorization: Bearer <INSTRUCTOR_TOKEN>
```

---

## 🛡️ Admin Endpoints

### Get Users
```http
GET /api/admin/users?role=student
Authorization: Bearer <ADMIN_TOKEN>
```

### Create User
```http
POST /api/admin/users
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "name": "New Student",
  "email": "student@uni.edu",
  "password": "Password123!",
  "role": "student",
  "studentId": "CS2024001",
  "department": "Computer Science",
  "year": 3,
  "gender": "male",
  "phone": "+20 1001234567",
  "address": "Cairo, Egypt"
}
```

### Update User
```http
PUT /api/admin/users/{userId}
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "name": "Updated Name",
  "status": "active"
}
```

### Toggle User Status
```http
PATCH /api/admin/users/{userId}/toggle-status
Authorization: Bearer <ADMIN_TOKEN>
```

### Dashboard
```http
GET /api/admin/dashboard
Authorization: Bearer <ADMIN_TOKEN>
```

### Get Courses
```http
GET /api/admin/courses
Authorization: Bearer <ADMIN_TOKEN>
```

### Create Course
```http
POST /api/admin/courses
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "code": "CS301",
  "name": "Information Retrieval Systems",
  "instructor": "Dr. Sarah Ahmed",
  "year": 3,
  "credits": 3,
  "color": "#818cf8",
  "description": "Course description",
  "prerequisites": ["CS201", "CS202"],
  "semester": "Fall 2025"
}
```

### Get Schedule Sessions
```http
GET /api/admin/schedule/sessions
Authorization: Bearer <ADMIN_TOKEN>
```

### Create Schedule Session
```http
POST /api/admin/schedule/sessions
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "year": 3,
  "group": "A",
  "code": "CS301",
  "name": "Information Retrieval Systems",
  "day": "Saturday",
  "start": 10.0,
  "end": 12.0,
  "type": "Lecture",
  "room": "Hall 301",
  "color": "#818cf8",
  "instructor": "Dr. Sarah Ahmed"
}
```

### Get Exam Schedules
```http
GET /api/admin/schedule/exams
Authorization: Bearer <ADMIN_TOKEN>
```

### Create Exam Schedule
```http
POST /api/admin/schedule/exams
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "year": 3,
  "type": "midterm",
  "code": "CS301",
  "name": "Information Retrieval Systems",
  "date": "2025-03-20",
  "time": "10:00 AM",
  "hall": "Hall 144",
  "duration": 2,
  "color": "#818cf8"
}
```

### Bulk Save Schedule
```http
POST /api/admin/schedule/save
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "sessions": [...],
  "exams": [...]
}
```

### Get Registration Settings
```http
GET /api/admin/registration/settings
Authorization: Bearer <ADMIN_TOKEN>
```

### Update Registration Settings
```http
PUT /api/admin/registration/settings
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "open": true,
  "allowedYears": [3, 4],
  "maxCreditsPerStudent": {
    "3": 18,
    "4": 21
  }
}
```

### Open Registration
```http
POST /api/admin/registration/open
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "allowedYears": [3, 4],
  "maxCreditsPerStudent": {
    "3": 18,
    "4": 21
  }
}
```

### Close Registration
```http
POST /api/admin/registration/close
Authorization: Bearer <ADMIN_TOKEN>
```

### Get Student's Courses
```http
GET /api/admin/students/{studentId}/courses
Authorization: Bearer <ADMIN_TOKEN>
```

### Add Course to Student
```http
POST /api/admin/students/{studentId}/courses
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "courseCode": "CS301"
}
```

### Remove Course from Student
```http
DELETE /api/admin/students/{studentId}/courses/CS301
Authorization: Bearer <ADMIN_TOKEN>
```

---

## 🤖 AI Tools Endpoints

### Chat (Streaming)
```http
POST /api/chat
Authorization: Bearer <TOKEN>
Content-Type: application/json

{
  "message": "Explain photosynthesis",
  "history": [
    {
      "role": "user",
      "content": "Hello"
    },
    {
      "role": "assistant",
      "content": "Hi there!"
    }
  ]
}

Response: Server-Sent Events stream
data: {"content": "Photosynthesis is..."}
```

### Extract Content
```http
POST /api/extract
Authorization: Bearer <TOKEN>
Content-Type: multipart/form-data

[file binary data]
type=pdf

Response:
{
  "content": "Extracted text...",
  "summary": "Brief summary...",
  "metadata": {
    "pages": 5,
    "size": "2.5 MB"
  }
}
```

### Generate Content
```http
POST /api/generate
Authorization: Bearer <TOKEN>
Content-Type: application/json

{
  "type": "flashcards",
  "content": "Photosynthesis is the process...",
  "count": 10
}

Response:
{
  "type": "flashcards",
  "content": [
    {
      "front": "What is photosynthesis?",
      "back": "The process by which plants convert light..."
    }
  ]
}
```

---

## Rate Limiting
- **General endpoints**: 100 requests/minute per IP
- **AI/Chat endpoints**: 10 requests/minute per IP
- **Response when limited**: HTTP 429 Too Many Requests

## Error Codes
- `VALIDATION_ERROR` - Input validation failed
- `UNAUTHORIZED` - Missing or invalid token
- `NOT_FOUND` - Resource not found
- `INVALID_OPERATION` - Business logic violation
- `CONFLICT` - Resource already exists
- `INTERNAL_SERVER_ERROR` - Server error

---

**Last Updated**: 2025
**Version**: 1.0
