# Frontend Integration Guide

## Base Configuration

### API Endpoints
```javascript
// Frontend should use these URLs
const API_BASE_URL = 'http://localhost:8000/api';
const AI_API_URL = 'http://localhost:8000';
```

### Authentication Flow
```
1. User submits login credentials
   POST /api/auth/login
   
2. Backend returns JWT token + user object
   Response: { token, user: { id, name, role, ... } }
   
3. Frontend stores token in localStorage
   localStorage.setItem('token', response.token);
   
4. Frontend includes token in ALL subsequent requests
   headers: {
     'Authorization': 'Bearer ' + localStorage.getItem('token'),
     'Content-Type': 'application/json'
   }
   
5. For logout
   POST /api/auth/logout (sends current token)
   Frontend clears localStorage
   
6. For token refresh
   POST /api/auth/refresh (sends current token)
   Backend returns new token
   Frontend updates localStorage
```

---

## HTTP Headers

### Standard Headers (All Requests)
```javascript
{
  'Content-Type': 'application/json',
  'Authorization': 'Bearer <JWT_TOKEN>'
}
```

### File Upload Headers
```javascript
{
  'Authorization': 'Bearer <JWT_TOKEN>'
  // Note: Content-Type is NOT set for multipart/form-data
  // (browser will set it automatically with boundary)
}
```

### Streaming Headers (Chat)
```javascript
{
  'Authorization': 'Bearer <JWT_TOKEN>',
  'Accept': 'text/event-stream'
}
```

---

## Error Handling

### Error Response Format
```json
{
  "error": "Human-readable error message",
  "code": "ERROR_CODE"
}
```

### HTTP Status Codes
```
200 OK - Successful request
201 Created - Resource created
400 Bad Request - Validation failed or business rule violation
401 Unauthorized - Missing or invalid token
403 Forbidden - Insufficient permissions
404 Not Found - Resource doesn't exist
409 Conflict - Resource already exists
422 Unprocessable Entity - Semantic validation error
429 Too Many Requests - Rate limit exceeded
500 Internal Server Error - Server error
```

### Common Error Codes
```
VALIDATION_ERROR - Input validation failed
UNAUTHORIZED - Missing or invalid token
NOT_FOUND - Resource not found
INVALID_OPERATION - Business logic violation
CONFLICT - Duplicate resource
ACCOUNT_INACTIVE - User account is inactive
PREREQUISITE_NOT_MET - Course prerequisite not met
CREDIT_LIMIT_EXCEEDED - Student exceeded credit limit
REGISTRATION_CLOSED - Registration period closed
ALREADY_SUBMITTED - Already submitted quiz/assignment
QUOTA_EXCEEDED - Quota exceeded (e.g., course full)
```

### Error Handling Example
```javascript
async function apiCall(endpoint, options = {}) {
  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json',
        ...options.headers
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw {
        status: response.status,
        message: error.error,
        code: error.code
      };
    }

    return await response.json();
  } catch (error) {
    // Handle error appropriately
    if (error.status === 401) {
      // Redirect to login
    } else if (error.status === 429) {
      // Show rate limit message
    } else {
      // Show generic error
    }
    throw error;
  }
}
```

---

## Student Module Integration

### Login Flow
```javascript
// 1. User clicks login
const loginResponse = await apiCall('/auth/login', {
  method: 'POST',
  body: JSON.stringify({
    email: 'student@uni.edu',
    password: 'Password123!'
  })
});

// 2. Response shape
{
  token: 'eyJhbGc...',
  user: {
    id: 'CS2024001',
    name: 'Ahmed Mohammed',
    email: 'student@uni.edu',
    role: 'student',
    department: 'Computer Science',
    year: '3',
    gender: 'male',
    phone: '+20 1001234567',
    address: 'Cairo, Egypt',
    avatar: 'base64_data_or_url',
    entryYear: 2022,
    dob: '2003-05-15'
  }
}

// 3. Store for later use
localStorage.setItem('token', loginResponse.token);
localStorage.setItem('user', JSON.stringify(loginResponse.user));
```

### Get Student Courses
```javascript
// GET /api/student/courses
const courses = await apiCall('/student/courses');

// Response: Array of StudentCourseDto
[
  {
    id: 'CS301',
    code: 'CS301',
    name: 'Information Retrieval Systems',
    instructor: 'Dr. Sarah Ahmed',
    color: '#818cf8',
    shade: 'rgba(129,140,248,0.15)',
    credits: 3,
    level: 3,
    semester: 'Fall 2025',
    description: 'Course description...',
    progress: 45,
    students: 25,
    icon: '📚'
  }
]
```

### Get Course Detail
```javascript
// GET /api/student/courses/:courseId
const courseDetail = await apiCall(`/student/courses/CS301`);

// Response: FullCourseDetailDto
{
  meta: {
    id: 'CS301',
    code: 'CS301',
    name: 'Information Retrieval Systems',
    instructor: 'Dr. Sarah Ahmed',
    color: '#818cf8',
    shade: 'rgba(129,140,248,0.15)',
    credits: 3,
    level: 3,
    semester: 'Fall 2025',
    description: '...',
    progress: 45
  },
  lectures: [
    {
      id: 'lec1',
      week: 1,
      title: 'Introduction to IR',
      type: 'video',
      duration: '45 min',
      date: 'Jan 15, 2025',
      size: '120 MB',
      watched: false
    }
  ],
  assignments: [
    {
      id: 'asg1',
      title: 'Assignment 1',
      deadline: '2025-02-15',
      max: 20,
      grade: null,
      status: 'pending',
      types: ['pdf', 'zip'],
      file: null
    }
  ],
  quizzes: [
    {
      id: 'q1',
      title: 'Quiz 1',
      date: '2025-02-01',
      duration: '30 min',
      questions: 10,
      max: 10,
      score: null,
      status: 'available',
      deadline: '2025-02-01T10:00:00'
    }
  ],
  midterm: {
    date: '2025-03-20',
    time: '10:00 AM – 12:00 PM',
    room: 'Hall 144 · Building B',
    published: false,
    grade: null,
    max: 50
  }
}
```

### Submit Quiz
```javascript
// POST /api/student/quizzes/:quizId/submit
const submission = await apiCall(`/student/quizzes/q1/submit`, {
  method: 'POST',
  body: JSON.stringify({
    answers: [0, 2, 1, 3, 2, 0, 1, 3, 2, 1]  // Indices of selected options
  })
});

// Response: QuizSubmitResponseDto
{
  score: 8,
  max: 10,
  submitted: true,
  graded: true
}
```

### Submit Assignment
```javascript
// POST /api/student/assignments/:assignmentId/submit
const formData = new FormData();
formData.append('file', fileInput.files[0]);

const submission = await fetch(
  `${API_BASE_URL}/student/assignments/asg1/submit`,
  {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${getToken()}`
      // Don't set Content-Type for FormData
    },
    body: formData
  }
);

// Response: SubmissionResponseDto
{
  message: 'Assignment submitted successfully',
  status: 'pending'
}
```

### Register for Course
```javascript
// POST /api/student/registration
const result = await apiCall('/student/registration', {
  method: 'POST',
  body: JSON.stringify({
    courseCode: 'CS301'
  })
});

// Response: RegistrationResponseDto
{
  message: 'Course registered successfully',
  course: {
    code: 'CS301',
    name: 'Information Retrieval Systems',
    credits: 3,
    instructor: 'Dr. Sarah Ahmed',
    year: 3,
    prerequisites: ['CS201', 'CS202'],
    status: 'registered',
    color: '#818cf8'
  }
}
```

### Get Academic Summary
```javascript
// GET /api/student/academic-summary
const summary = await apiCall('/student/academic-summary');

// Response: AcademicSummaryDto
{
  gpa: 3.45,
  completedCredits: 42,
  remainingCredits: 36,
  currentYear: 3,
  standing: 'Very Good',
  coursesThisSemester: 5,
  activeAssignments: 2,
  upcomingQuizzes: 3,
  nextDeadline: '2025-02-15',
  overallRank: 8,
  totalStudents: 120
}
```

---

## Instructor Module Integration

### Get Dashboard
```javascript
// GET /api/instructor/dashboard
const dashboard = await apiCall('/instructor/dashboard');

// Response: InstructorDashboardDto
{
  courses: [
    {
      id: 'CS301',
      code: 'CS301',
      name: 'Information Retrieval Systems',
      color: '#818cf8',
      icon: '📚',
      students: 25,
      progress: 40
    }
  ],
  gradeSummary: {
    'CS301': {
      pending: 5,
      approved: 12,
      rejected: 0,
      avg: '14.2'
    }
  },
  activity: [
    {
      id: 'act1',
      student: 'Ahmed Mohammed',
      detail: 'submitted Assignment 2',
      course: 'CS301',
      icon: '📝',
      color: '#818cf8',
      time: '2h'
    }
  ],
  upcoming: [
    {
      id: 'evt1',
      title: 'Midterm Exam',
      date: '2025-03-20',
      time: '10:00 AM',
      room: 'Hall 144',
      icon: '📋',
      color: '#818cf8'
    }
  ]
}
```

### Upload Course Material
```javascript
// POST /api/instructor/courses/:courseId/materials
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('title', 'Lecture 1');
formData.append('description', 'Week 1 lecture');
formData.append('type', 'video');

const result = await fetch(
  `${API_BASE_URL}/instructor/courses/CS301/materials`,
  {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${getToken()}`
    },
    body: formData
  }
);

// Response: InstructorMaterialDto
{
  id: 'mat1',
  title: 'Lecture 1',
  type: 'video',
  url: '/uploads/materials/lecture1.mp4',
  size: '250 MB',
  date: '2025-01-15',
  downloads: 0
}
```

### Grade Submission
```javascript
// POST /api/instructor/submissions/:submissionId/grade
const result = await apiCall(`/instructor/submissions/sub1/grade`, {
  method: 'POST',
  body: JSON.stringify({
    grade: 18,
    feedback: 'Well done! Good analysis.'
  })
});

// Response: SubmissionDto
{
  id: 'sub1',
  assignmentId: 'asg1',
  assignmentTitle: 'Assignment 1',
  courseCode: 'CS301',
  studentId: 'CS2024001',
  studentName: 'Ahmed Mohammed',
  submittedAt: '2025-02-01 14:30',
  fileName: 'solution.pdf',
  status: 'approved',
  grade: 18,
  feedback: 'Well done! Good analysis.'
}
```

---

## Admin Module Integration

### Create User
```javascript
// POST /api/admin/users
const user = await apiCall('/admin/users', {
  method: 'POST',
  body: JSON.stringify({
    name: 'New Student',
    email: 'new@uni.edu',
    password: 'Password123!',
    role: 'student',
    studentId: 'CS2024005',
    department: 'Computer Science',
    year: 3,
    gender: 'male',
    phone: '+20 1001234567',
    address: 'Cairo, Egypt'
  })
});

// Response: AdminUserDto
{
  id: 'CS2024005',
  name: 'New Student',
  email: 'new@uni.edu',
  role: 'student',
  department: 'Computer Science',
  status: 'active',
  createdAt: '2025-01-15T10:30:00Z'
}
```

### Save Schedule
```javascript
// POST /api/admin/schedule/save
const result = await apiCall('/admin/schedule/save', {
  method: 'POST',
  body: JSON.stringify({
    sessions: [
      {
        id: 'sess1',
        year: 3,
        group: 'A',
        code: 'CS301',
        name: 'Information Retrieval Systems',
        day: 'Saturday',
        start: 10.0,
        end: 12.0,
        type: 'Lecture',
        room: 'Hall 301',
        color: '#818cf8',
        instructor: 'Dr. Sarah Ahmed'
      }
    ],
    exams: [
      {
        id: 'exam1',
        year: 3,
        type: 'midterm',
        code: 'CS301',
        name: 'Information Retrieval Systems',
        date: '2025-03-20',
        time: '10:00 AM',
        hall: 'Hall 144',
        duration: 2,
        color: '#818cf8'
      }
    ]
  })
});

// Response: SaveScheduleResponseDto
{
  message: 'Schedule saved successfully',
  saved: 5
}
```

---

## AI Tools Integration

### Chat (Streaming)
```javascript
// POST /api/chat (Server-Sent Events)
const eventSource = new EventSource(
  `${API_BASE_URL}/chat`,
  {
    headers: {
      'Authorization': `Bearer ${getToken()}`
    }
  }
);

// Note: EventSource only supports GET, so you need to use fetch with ReadableStream:
const response = await fetch(`${API_BASE_URL}/chat`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${getToken()}`,
    'Content-Type': 'application/json',
    'Accept': 'text/event-stream'
  },
  body: JSON.stringify({
    message: 'Explain photosynthesis',
    history: [
      { role: 'user', content: 'Hello' },
      { role: 'assistant', content: 'Hi there!' }
    ]
  })
});

// Read streaming response
const reader = response.body.getReader();
const decoder = new TextDecoder();

while (true) {
  const { done, value } = await reader.read();
  if (done) break;
  
  const chunk = decoder.decode(value);
  // Parse SSE data: data: {"content": "..."}
  const lines = chunk.split('\n');
  for (const line of lines) {
    if (line.startsWith('data: ')) {
      const data = JSON.parse(line.slice(6));
      // Update UI with data.content
    }
  }
}
```

### Extract Content
```javascript
// POST /api/extract
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('type', 'pdf');

const result = await fetch(
  `${API_BASE_URL}/extract`,
  {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${getToken()}`
    },
    body: formData
  }
);

// Response: ExtractResponseDto
{
  content: 'Extracted text from PDF...',
  summary: 'Brief summary of content...',
  metadata: {
    pages: 5,
    size: '2.5 MB',
    format: 'pdf'
  }
}
```

### Generate Content
```javascript
// POST /api/generate
const result = await apiCall('/generate', {
  method: 'POST',
  body: JSON.stringify({
    type: 'flashcards',  // or 'summary', 'quiz'
    content: 'Photosynthesis is the process by which plants...',
    count: 10
  })
});

// Response for flashcards:
{
  type: 'flashcards',
  content: [
    {
      front: 'What is photosynthesis?',
      back: 'The process by which plants convert light energy...'
    },
    {
      front: 'What are the inputs for photosynthesis?',
      back: 'Water, carbon dioxide, and sunlight'
    }
  ]
}

// Response for summary:
{
  type: 'summary',
  content: {
    title: 'Photosynthesis Overview',
    sections: [
      {
        heading: 'Definition',
        content: 'Photosynthesis is...'
      },
      {
        heading: 'Process',
        content: 'The process occurs in...'
      }
    ]
  }
}

// Response for quiz:
{
  type: 'quiz',
  content: {
    title: 'Photosynthesis Quiz',
    questions: [
      {
        q: 'What is the primary input for photosynthesis?',
        options: ['Oxygen', 'Water', 'Carbon dioxide', 'All of above'],
        answer: 2
      }
    ]
  }
}
```

---

## Rate Limiting

### Default Limits
- **General endpoints**: 100 requests/minute per IP
- **AI/Chat endpoints**: 10 requests/minute per IP

### Response When Limited
```
HTTP 429 Too Many Requests

{
  "error": "Too many requests from this IP",
  "code": "RATE_LIMIT_EXCEEDED"
}
```

### Best Practices
1. Implement exponential backoff
2. Cache results when possible
3. Batch requests where applicable
4. Use WebSockets for real-time updates

---

## Token Refresh Strategy

### Token Lifecycle
1. **Issue**: JWT token valid for 1 hour (configure in appsettings.json)
2. **Near Expiry**: Frontend detects token expiring in 5 minutes
3. **Refresh**: Frontend calls POST /api/auth/refresh
4. **Update**: Frontend stores new token
5. **Fallback**: If refresh fails, redirect to login

### Implementation Example
```javascript
let tokenRefreshTimer;

function startTokenRefreshTimer() {
  const token = getToken();
  const decoded = jwtDecode(token);
  const expiresIn = decoded.exp * 1000 - Date.now();
  
  // Refresh 5 minutes before expiry
  const refreshTime = expiresIn - (5 * 60 * 1000);
  
  if (tokenRefreshTimer) clearTimeout(tokenRefreshTimer);
  
  tokenRefreshTimer = setTimeout(async () => {
    try {
      const response = await apiCall('/auth/refresh', {
        method: 'POST'
      });
      localStorage.setItem('token', response.token);
      startTokenRefreshTimer(); // Restart timer with new token
    } catch (error) {
      // Refresh failed, redirect to login
      redirectToLogin();
    }
  }, refreshTime);
}

// Call on app init and after login
startTokenRefreshTimer();
```

---

## Debugging Tips

### Check Token
```javascript
// Decode JWT to see claims
const token = localStorage.getItem('token');
const decoded = jwtDecode(token);
console.log('User ID:', decoded.sub);
console.log('Role:', decoded.role);
console.log('Expires:', new Date(decoded.exp * 1000));
```

### Check API Response
```javascript
// Log all API responses
const originalFetch = window.fetch;
window.fetch = async (...args) => {
  const response = await originalFetch(...args);
  console.log(`${args[1]?.method || 'GET'} ${args[0]}: ${response.status}`);
  return response;
};
```

### Monitor Network
- Open DevTools → Network tab
- Filter by XHR/Fetch
- Check request/response headers and body
- Verify Authorization header is present

---

## Common Issues & Solutions

### "Unauthorized" on every request
**Issue**: Token not being sent
**Solution**: Check Authorization header is set correctly

### CORS Error
**Issue**: Backend not allowing requests
**Solution**: Verify CORS configured for your domain

### File Upload Fails
**Issue**: Content-Type header set to application/json
**Solution**: Remove Content-Type header for multipart/form-data

### Rate Limited
**Issue**: Getting 429 responses
**Solution**: Implement backoff and cache results

### Token Expired
**Issue**: Getting 401 after 1 hour
**Solution**: Implement token refresh timer

---

**Last Updated**: 2025
**Version**: 1.0
