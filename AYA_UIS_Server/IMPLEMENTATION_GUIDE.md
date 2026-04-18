# Implementation Guidelines for AYA University IS Backend

## Overview
This document provides guidance for implementing the TODO items in the controllers and services.

## General Principles

### 1. Data Flow Pattern
```
Controller → Service/Manager → Repository → Database
```

### 2. Response Format
Always wrap responses properly using the existing `FrontendLoginResponseDto` pattern or direct DTOs.

### 3. Error Handling
Use proper HTTP status codes and error format:
```csharp
if (!user.IsActive)
    return BadRequest(new { error = "User account is inactive", code = "ACCOUNT_INACTIVE" });
```

---

## Authentication Implementation

### POST /api/auth/refresh
```csharp
[HttpPost("refresh")]
[Authorize]
public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken()
{
    // 1. Extract current user from HttpContext.User
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // 2. Validate user still exists and token is not revoked
    var user = await _serviceManager.AuthenticationService.GetUserByIdAsync(userId);
    if (user == null) return Unauthorized();
    
    // 3. Generate new JWT token
    var newToken = await _serviceManager.AuthenticationService.GenerateJwtAsync(user);
    
    // 4. Return new token
    return Ok(new RefreshTokenResponseDto { Token = newToken });
}
```

### POST /api/auth/logout
```csharp
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout()
{
    // 1. Extract token from Authorization header
    var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    
    // 2. Add token to revocation list (implement TokenRevocationService)
    await _tokenRevocationService.RevokeTokenAsync(token);
    
    // 3. Return success
    return Ok(new { message = "Logged out successfully" });
}
```

---

## Student Profile Implementation

### GET /api/student/profile
```csharp
[HttpGet("profile")]
public async Task<ActionResult<StudentProfileDto>> GetProfile()
{
    // Extract student ID from JWT claim
    var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(studentId))
        return Unauthorized();
    
    // Get user from database
    var user = await _unitOfWork.Users.GetByIdAsync(studentId);
    if (user == null)
        return NotFound(new { error = "User not found" });
    
    // Map to DTO
    var profileDto = _mapper.Map<StudentProfileDto>(user);
    return Ok(profileDto);
}
```

---

## Course Enrollment Implementation

### GET /api/student/courses
```csharp
[HttpGet("courses")]
public async Task<ActionResult<List<StudentCourseDto>>> GetCourses()
{
    var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Get enrollments for student
    var enrollments = await _unitOfWork.Enrollments
        .GetAsync(e => e.StudentId == studentId && e.Status == "active");
    
    // Get courses with progress and instructor info
    var courses = new List<StudentCourseDto>();
    foreach (var enrollment in enrollments)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(enrollment.CourseId);
        var progress = CalculateCourseProgress(enrollment.CourseId, studentId);
        
        courses.Add(new StudentCourseDto
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Instructor = course.InstructorName,
            Color = course.Color,
            Shade = course.ColorShade,
            Credits = course.Credits,
            Level = course.Level,
            Semester = course.Semester,
            Description = course.Description,
            Progress = progress,
            Students = course.StudentCount,
            Icon = course.Icon
        });
    }
    
    return Ok(courses);
}
```

---

## Academic Summary Implementation

### Calculating GPA and Standing

The specification provides these rules:

```csharp
public string CalculateStanding(decimal gpa)
{
    return gpa switch
    {
        >= 3.5m => "Excellent",
        >= 3.0m => "Very Good",
        >= 2.5m => "Good",
        >= 2.0m => "Pass",
        >= 1.5m => "Academic Warning",
        _ => "Academic Probation"
    };
}

public decimal CalculateLetterGradePoints(int percentage)
{
    return percentage switch
    {
        >= 97 => 4.0m,     // A+
        >= 93 => 4.0m,     // A
        >= 90 => 3.7m,     // A-
        >= 87 => 3.3m,     // B+
        >= 83 => 3.0m,     // B
        >= 80 => 2.7m,     // B-
        >= 77 => 2.3m,     // C+
        >= 73 => 2.0m,     // C
        >= 70 => 1.7m,     // C-
        >= 67 => 1.3m,     // D+
        >= 60 => 1.0m,     // D
        _ => 0.0m          // F
    };
}
```

---

## Quiz Implementation

### GET /api/student/courses/:courseId/quizzes/:quizId
```csharp
[HttpGet("courses/{courseId}/quizzes/{quizId}")]
public async Task<ActionResult<QuizDetailDto>> GetQuizDetail(string courseId, string quizId)
{
    var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Get quiz
    var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
    if (quiz == null) return NotFound();
    
    // Get questions
    var questions = await _unitOfWork.QuizQuestions
        .GetAsync(q => q.QuizId == quizId);
    
    // Get student's attempt if exists
    var attempt = await _unitOfWork.QuizSubmissions
        .FirstOrDefaultAsync(s => s.QuizId == quizId && s.StudentId == studentId);
    
    // Map to DTO
    var quizDto = new QuizDetailDto
    {
        Id = quiz.Id,
        CourseId = courseId,
        Title = quiz.Title,
        Duration = quiz.Duration,
        Questions = questions.Count,
        Max = questions.Count * quiz.GradePerQuestion,
        Score = attempt?.Score,
        Status = GetQuizStatus(quiz),
        Deadline = quiz.Deadline.ToString("O"),
        Mcq = questions.Select(q => new MCQDto
        {
            Q = q.Text,
            Opts = q.Options.Select(o => o.Text).ToList(),
            Ans = q.CorrectAnswer
        }).ToList()
    };
    
    return Ok(quizDto);
}

private string GetQuizStatus(Quiz quiz)
{
    var now = DateTime.UtcNow;
    if (now < quiz.StartDate) return "upcoming";
    if (now > quiz.Deadline) return "completed";
    return "available";
}
```

### POST /api/student/quizzes/:quizId/submit
```csharp
[HttpPost("quizzes/{quizId}/submit")]
public async Task<ActionResult<QuizSubmitResponseDto>> SubmitQuiz(
    string quizId,
    [FromBody] SubmitQuizDto dto)
{
    var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Check if already submitted
    var existing = await _unitOfWork.QuizSubmissions
        .FirstOrDefaultAsync(s => s.QuizId == quizId && s.StudentId == studentId);
    
    if (existing != null)
        return BadRequest(new { error = "Already submitted this quiz" });
    
    // Get quiz and questions
    var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
    var questions = await _unitOfWork.QuizQuestions
        .GetAsync(q => q.QuizId == quizId);
    
    // Check deadline
    if (DateTime.UtcNow > quiz.Deadline)
        return BadRequest(new { error = "Quiz deadline has passed" });
    
    // Calculate score
    int score = 0;
    for (int i = 0; i < dto.Answers.Count; i++)
    {
        if (dto.Answers[i] == questions[i].CorrectAnswer)
            score += quiz.GradePerQuestion;
    }
    
    // Save submission
    var submission = new QuizSubmission
    {
        QuizId = quizId,
        StudentId = studentId,
        Answers = JsonSerializer.Serialize(dto.Answers),
        Score = score,
        SubmittedAt = DateTime.UtcNow
    };
    
    await _unitOfWork.QuizSubmissions.AddAsync(submission);
    await _unitOfWork.SaveChangesAsync();
    
    return Ok(new QuizSubmitResponseDto
    {
        Score = score,
        Max = questions.Count * quiz.GradePerQuestion,
        Submitted = true,
        Graded = true
    });
}
```

---

## Assignment Submission Implementation

### POST /api/student/assignments/:assignmentId/submit
```csharp
[HttpPost("assignments/{assignmentId}/submit")]
[Consumes("multipart/form-data")]
public async Task<ActionResult<SubmissionResponseDto>> SubmitAssignment(
    string assignmentId,
    IFormFile file)
{
    var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Validate file
    if (file == null || file.Length == 0)
        return BadRequest(new { error = "File is required" });
    
    // Get assignment
    var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
    if (assignment == null) return NotFound();
    
    // Check deadline
    if (DateTime.UtcNow > assignment.Deadline)
        return BadRequest(new { error = "Assignment deadline has passed" });
    
    // Check file type
    var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
    if (!assignment.AllowedFormats.Contains(fileExtension))
        return BadRequest(new 
        { 
            error = $"Invalid file type. Allowed: {string.Join(", ", assignment.AllowedFormats)}" 
        });
    
    // Check file size (max 50MB)
    if (file.Length > 50 * 1024 * 1024)
        return BadRequest(new { error = "File size exceeds 50MB limit" });
    
    // Check existing submission
    var existing = await _unitOfWork.Submissions
        .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && 
                                  s.StudentId == studentId);
    
    if (existing != null)
        return BadRequest(new { error = "Already submitted this assignment" });
    
    // Upload file
    var fileName = $"{studentId}_{assignmentId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var filePath = await _fileService.SaveFileAsync(file, "assignments", fileName);
    
    // Create submission record
    var submission = new AssignmentSubmission
    {
        AssignmentId = assignmentId,
        StudentId = studentId,
        FilePath = filePath,
        FileName = file.FileName,
        SubmittedAt = DateTime.UtcNow,
        Status = "pending"
    };
    
    await _unitOfWork.Submissions.AddAsync(submission);
    await _unitOfWork.SaveChangesAsync();
    
    return Ok(new SubmissionResponseDto
    {
        Message = "Assignment submitted successfully",
        Status = "pending"
    });
}
```

---

## Course Registration Implementation

### Key Validation Rules

```csharp
public async Task<bool> CanRegisterAsync(string studentId, string courseCode)
{
    var student = await _unitOfWork.Users.GetByIdAsync(studentId);
    var course = await _unitOfWork.Courses.GetByCodeAsync(courseCode);
    
    // Rule 1: Check if already registered
    var existing = await _unitOfWork.Enrollments
        .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == course.Id);
    if (existing != null) return false;
    
    // Rule 2: Check prerequisites
    foreach (var prerequisite in course.Prerequisites)
    {
        var completed = await _unitOfWork.CourseResults
            .FirstOrDefaultAsync(r => r.StudentId == studentId && 
                                      r.Course.Code == prerequisite && 
                                      r.Grade >= 60);
        if (completed == null) return false;
    }
    
    // Rule 3: Check credit limit based on GPA
    var studentGpa = await _gpaCalculator.CalculateGpaAsync(studentId);
    var maxCredits = GetMaxCreditsForGpa(studentGpa);
    var currentCredits = await GetCurrentCreditsAsync(studentId);
    
    if (currentCredits + course.Credits > maxCredits) return false;
    
    // Rule 4: Check registration period
    var regSettings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
    if (!regSettings.Open) return false;
    if (!regSettings.AllowedYears.Contains(student.Year)) return false;
    
    return true;
}

private int GetMaxCreditsForGpa(decimal gpa)
{
    return gpa switch
    {
        >= 3.5m => 21,
        >= 3.0m => 18,
        >= 2.5m => 18,
        >= 2.0m => 15,
        >= 1.5m => 12,
        _ => 9
    };
}
```

---

## File Upload Helper

```csharp
public class FileService
{
    public async Task<string> SaveFileAsync(IFormFile file, string folderName, string fileName)
    {
        var uploadPath = Path.Combine("wwwroot", "uploads", folderName);
        
        // Create directory if not exists
        Directory.CreateDirectory(uploadPath);
        
        var filePath = Path.Combine(uploadPath, fileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        // Return relative URL
        return $"/uploads/{folderName}/{fileName}";
    }
    
    public bool DeleteFile(string filePath)
    {
        var fullPath = Path.Combine("wwwroot", filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }
        return false;
    }
}
```

---

## Mapper Configuration Example

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User to StudentProfileDto
        CreateMap<User, StudentProfileDto>()
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.Name))
            .ForMember(d => d.Year, o => o.MapFrom(s => s.StudyYears.FirstOrDefault()?.Year.ToString()))
            .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.ToString().ToLower()))
            .ForMember(d => d.Dob, o => o.MapFrom(s => s.DateOfBirth.ToString("yyyy-MM-dd")));
        
        // Course to StudentCourseDto
        CreateMap<Course, StudentCourseDto>()
            .ForMember(d => d.Icon, o => o.MapFrom(s => GetCourseIcon(s.Code)))
            .ForMember(d => d.Students, o => o.MapFrom(s => s.Enrollments.Count));
    }
}
```

---

## Testing Tips

### Unit Test Example
```csharp
[TestFixture]
public class StudentServiceTests
{
    [Test]
    public async Task GetProfile_WithValidStudentId_ReturnsProfile()
    {
        // Arrange
        var studentId = "CS2024001";
        var mockRepository = new Mock<IUserRepository>();
        var student = new User { Id = studentId, Name = "Test" };
        mockRepository.Setup(r => r.GetByIdAsync(studentId))
            .ReturnsAsync(student);
        
        // Act
        var result = await _service.GetProfileAsync(studentId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(studentId, result.Id);
    }
}
```

---

## Performance Considerations

1. **Use async/await** for all DB operations
2. **Eager load** related data when possible:
   ```csharp
   var courses = await _unitOfWork.Courses
       .GetAsync(filter: null, includeProperties: "Instructor,Department");
   ```

3. **Cache** frequently accessed data:
   ```csharp
   var courses = await _cacheService.GetOrSetAsync("courses", async () => 
       await _unitOfWork.Courses.GetAllAsync());
   ```

4. **Paginate** large result sets:
   ```csharp
   var pagedCourses = await _unitOfWork.Courses
       .GetPagedAsync(pageNumber: 1, pageSize: 20);
   ```

---

## Security Checklist

- ✅ Validate all user inputs
- ✅ Check authorization for every endpoint
- ✅ Use parameterized queries (EF Core does this)
- ✅ Hash passwords securely
- ✅ Validate file uploads
- ✅ Sanitize file names
- ✅ Use HTTPS in production
- ✅ Implement rate limiting
- ✅ Log security events
- ✅ Use CORS properly

---

**Version**: 1.0
**Last Updated**: 2025
