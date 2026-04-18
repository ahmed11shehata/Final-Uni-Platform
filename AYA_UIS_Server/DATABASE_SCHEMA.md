# Database Schema Reference

## Core Tables Required for Implementation

### Users Table
```sql
CREATE TABLE Users (
    Id NVARCHAR(128) PRIMARY KEY,
    UserName NVARCHAR(256) UNIQUE,
    Email NVARCHAR(256) UNIQUE,
    PasswordHash NVARCHAR(MAX),
    DisplayName NVARCHAR(256),
    Role NVARCHAR(50), -- student, instructor, admin
    Department NVARCHAR(256),
    Year INT, -- 1-4 for students, NULL for others
    Gender NVARCHAR(50),
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(512),
    Avatar NVARCHAR(MAX), -- Base64 or URL
    EntryYear INT,
    DateOfBirth DATE,
    Status NVARCHAR(50), -- active, inactive
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### Courses Table
```sql
CREATE TABLE Courses (
    Id NVARCHAR(128) PRIMARY KEY,
    Code NVARCHAR(50) UNIQUE,
    Name NVARCHAR(256),
    Description NVARCHAR(MAX),
    InstructorId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    InstructorName NVARCHAR(256),
    Year INT, -- 1-4
    Level INT,
    Credits INT,
    Color NVARCHAR(50), -- Hex color
    ColorShade NVARCHAR(50), -- RGBA
    Icon NVARCHAR(10), -- Emoji
    Semester NVARCHAR(50), -- "Fall 2025", "Spring 2025"
    Status NVARCHAR(50), -- active, inactive
    StudentCount INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### Enrollments Table
```sql
CREATE TABLE Enrollments (
    Id NVARCHAR(128) PRIMARY KEY,
    StudentId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    Status NVARCHAR(50), -- active, completed, failed
    EnrolledAt DATETIME2 DEFAULT GETUTCDATE(),
    UNIQUE(StudentId, CourseId)
);
```

### CourseResults Table
```sql
CREATE TABLE CourseResults (
    Id NVARCHAR(128) PRIMARY KEY,
    StudentId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    Grade INT, -- 0-100
    LetterGrade NVARCHAR(2), -- A+, A, A-, B+, etc.
    GradePoints DECIMAL(3,2), -- 4.0, 3.7, 3.3, etc.
    Semester NVARCHAR(50),
    Year INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### Assignments Table
```sql
CREATE TABLE Assignments (
    Id NVARCHAR(128) PRIMARY KEY,
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    Title NVARCHAR(256),
    Description NVARCHAR(MAX),
    Deadline DATETIME2,
    MaxGrade INT,
    AllowedFormats NVARCHAR(MAX), -- JSON array: ["pdf", "zip", "doc"]
    Status NVARCHAR(50), -- draft, published, closed
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### AssignmentSubmissions Table
```sql
CREATE TABLE AssignmentSubmissions (
    Id NVARCHAR(128) PRIMARY KEY,
    AssignmentId NVARCHAR(128) FOREIGN KEY REFERENCES Assignments(Id),
    StudentId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    FilePath NVARCHAR(MAX),
    FileName NVARCHAR(256),
    SubmittedAt DATETIME2,
    Status NVARCHAR(50), -- pending, approved, rejected
    Grade INT,
    Feedback NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### Quizzes Table
```sql
CREATE TABLE Quizzes (
    Id NVARCHAR(128) PRIMARY KEY,
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    Title NVARCHAR(256),
    Description NVARCHAR(MAX),
    Duration INT, -- Minutes
    GradePerQuestion INT,
    StartDate DATETIME2,
    Deadline DATETIME2,
    Status NVARCHAR(50), -- draft, published, closed
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### QuizQuestions Table
```sql
CREATE TABLE QuizQuestions (
    Id NVARCHAR(128) PRIMARY KEY,
    QuizId NVARCHAR(128) FOREIGN KEY REFERENCES Quizzes(Id),
    Text NVARCHAR(MAX),
    Options NVARCHAR(MAX), -- JSON array of 4 options
    CorrectAnswer INT, -- 0-3 (index of correct option)
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### QuizSubmissions Table
```sql
CREATE TABLE QuizSubmissions (
    Id NVARCHAR(128) PRIMARY KEY,
    QuizId NVARCHAR(128) FOREIGN KEY REFERENCES Quizzes(Id),
    StudentId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    Answers NVARCHAR(MAX), -- JSON array of answers
    Score INT,
    SubmittedAt DATETIME2 DEFAULT GETUTCDATE(),
    UNIQUE(QuizId, StudentId)
);
```

### ScheduleSessions Table
```sql
CREATE TABLE ScheduleSessions (
    Id NVARCHAR(128) PRIMARY KEY,
    Year INT, -- 1-4
    Group NVARCHAR(50), -- "A", "B"
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    Day NVARCHAR(50), -- Saturday, Sunday, etc.
    StartTime DECIMAL(5,2), -- 10.0, 10.5, 14.75 (decimal hour)
    EndTime DECIMAL(5,2),
    Type NVARCHAR(50), -- Lecture, Section, Lab
    Room NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### ExamSchedule Table
```sql
CREATE TABLE ExamSchedule (
    Id NVARCHAR(128) PRIMARY KEY,
    Year INT, -- 1-4
    Type NVARCHAR(50), -- midterm, final
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    Date DATE,
    Time NVARCHAR(50), -- "10:00 AM"
    Hall NVARCHAR(50),
    Duration INT, -- Hours
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### CourseMaterials Table
```sql
CREATE TABLE CourseMaterials (
    Id NVARCHAR(128) PRIMARY KEY,
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    Title NVARCHAR(256),
    Type NVARCHAR(50), -- video, pdf, lecture
    FilePath NVARCHAR(MAX),
    FileSize BIGINT, -- bytes
    Week INT,
    Downloads INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### ExamGrades Table
```sql
CREATE TABLE ExamGrades (
    Id NVARCHAR(128) PRIMARY KEY,
    StudentId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    ExamType NVARCHAR(50), -- midterm, final
    Grade INT,
    MaxGrade INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UNIQUE(StudentId, CourseId, ExamType)
);
```

### RegistrationSettings Table
```sql
CREATE TABLE RegistrationSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Open BIT,
    AllowedYears NVARCHAR(MAX), -- JSON array: [3, 4]
    MaxCreditsPerStudent NVARCHAR(MAX), -- JSON: {"3": 18, "4": 21}
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### ActivityLog Table
```sql
CREATE TABLE ActivityLog (
    Id NVARCHAR(128) PRIMARY KEY,
    UserId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    Type NVARCHAR(50), -- registration, grade, login, course_add
    Action NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### CoursePrerequisites Table
```sql
CREATE TABLE CoursePrerequisites (
    Id NVARCHAR(128) PRIMARY KEY,
    CourseId NVARCHAR(128) FOREIGN KEY REFERENCES Courses(Id),
    PrerequisiteCourseCode NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### AdminCourseLock Table (for admin overrides)
```sql
CREATE TABLE AdminCourseLock (
    Id NVARCHAR(128) PRIMARY KEY,
    StudentId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    CourseCode NVARCHAR(50),
    Locked BIT,
    Reason NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UNIQUE(StudentId, CourseCode)
);
```

### StudentCourseExceptions Table
```sql
CREATE TABLE StudentCourseExceptions (
    Id NVARCHAR(128) PRIMARY KEY,
    StudentId NVARCHAR(128) FOREIGN KEY REFERENCES Users(Id),
    CourseCode NVARCHAR(50),
    ExceptionType NVARCHAR(50), -- unlock, waive
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

---

## Key Indexes for Performance

```sql
-- Users Indexes
CREATE INDEX IDX_Users_Email ON Users(Email);
CREATE INDEX IDX_Users_Role ON Users(Role);
CREATE INDEX IDX_Users_Status ON Users(Status);

-- Courses Indexes
CREATE INDEX IDX_Courses_Code ON Courses(Code);
CREATE INDEX IDX_Courses_Year ON Courses(Year);
CREATE INDEX IDX_Courses_InstructorId ON Courses(InstructorId);

-- Enrollments Indexes
CREATE INDEX IDX_Enrollments_StudentId ON Enrollments(StudentId);
CREATE INDEX IDX_Enrollments_CourseId ON Enrollments(CourseId);
CREATE INDEX IDX_Enrollments_Status ON Enrollments(Status);

-- CourseResults Indexes
CREATE INDEX IDX_CourseResults_StudentId ON CourseResults(StudentId);
CREATE INDEX IDX_CourseResults_CourseId ON CourseResults(CourseId);
CREATE INDEX IDX_CourseResults_StudentCourse ON CourseResults(StudentId, CourseId);

-- Assignments Indexes
CREATE INDEX IDX_Assignments_CourseId ON Assignments(CourseId);
CREATE INDEX IDX_Assignments_Deadline ON Assignments(Deadline);

-- AssignmentSubmissions Indexes
CREATE INDEX IDX_AssignmentSubmissions_AssignmentId ON AssignmentSubmissions(AssignmentId);
CREATE INDEX IDX_AssignmentSubmissions_StudentId ON AssignmentSubmissions(StudentId);
CREATE INDEX IDX_AssignmentSubmissions_Status ON AssignmentSubmissions(Status);

-- Quizzes Indexes
CREATE INDEX IDX_Quizzes_CourseId ON Quizzes(CourseId);
CREATE INDEX IDX_Quizzes_Deadline ON Quizzes(Deadline);

-- QuizSubmissions Indexes
CREATE INDEX IDX_QuizSubmissions_QuizId ON QuizSubmissions(QuizId);
CREATE INDEX IDX_QuizSubmissions_StudentId ON QuizSubmissions(StudentId);

-- ScheduleSessions Indexes
CREATE INDEX IDX_ScheduleSessions_Year ON ScheduleSessions(Year);
CREATE INDEX IDX_ScheduleSessions_Day ON ScheduleSessions(Day);

-- ExamSchedule Indexes
CREATE INDEX IDX_ExamSchedule_Date ON ExamSchedule([Date]);
CREATE INDEX IDX_ExamSchedule_Year ON ExamSchedule(Year);

-- ActivityLog Indexes
CREATE INDEX IDX_ActivityLog_UserId ON ActivityLog(UserId);
CREATE INDEX IDX_ActivityLog_CreatedAt ON ActivityLog(CreatedAt);
```

---

## Query Examples

### Get Student's Current GPA
```sql
SELECT AVG(cr.GradePoints) as GPA
FROM CourseResults cr
WHERE cr.StudentId = @studentId 
  AND cr.Year = (SELECT MAX(Year) FROM CourseResults WHERE StudentId = @studentId)
```

### Get Available Courses for Student Registration
```sql
SELECT c.*, COUNT(e.Id) as EnrolledCount
FROM Courses c
LEFT JOIN Enrollments e ON c.Id = e.CourseId AND e.StudentId = @studentId
WHERE c.Year = (SELECT Year FROM Users WHERE Id = @studentId)
  AND NOT EXISTS (
    SELECT 1 FROM Enrollments WHERE StudentId = @studentId AND CourseId = c.Id AND Status = 'active'
  )
GROUP BY c.Id, c.Code, c.Name, c.Year
ORDER BY c.Code
```

### Get Student's Transcript by Year
```sql
SELECT 
    cr.Year,
    cr.Course.Semester,
    cr.Course.Code,
    cr.Course.Name,
    cr.Grade,
    cr.LetterGrade,
    cr.GradePoints,
    cr.Course.Credits,
    AVG(cr.GradePoints) OVER (PARTITION BY cr.Year) as YearGPA
FROM CourseResults cr
WHERE cr.StudentId = @studentId
ORDER BY cr.Year DESC, cr.Course.Code
```

### Get Course with All Details
```sql
SELECT 
    c.*,
    COUNT(DISTINCT e.StudentId) as StudentCount,
    COUNT(DISTINCT a.Id) as AssignmentCount,
    COUNT(DISTINCT q.Id) as QuizCount
FROM Courses c
LEFT JOIN Enrollments e ON c.Id = e.CourseId
LEFT JOIN Assignments a ON c.Id = a.CourseId
LEFT JOIN Quizzes q ON c.Id = q.CourseId
WHERE c.Id = @courseId
GROUP BY c.Id, c.Code, c.Name, c.Description, c.InstructorId, c.InstructorName, c.Year
```

---

## Seed Data Template

```csharp
// Add departments
var depts = new[]
{
    new Department { Id = 1, Name = "Computer Science" },
    new Department { Id = 2, Name = "Engineering" }
};

// Add users
var users = new[]
{
    new User 
    { 
        Id = "CS2024001", 
        UserName = "ahmed.mohammed",
        Email = "ahmed.mohammed@uni.edu",
        DisplayName = "Ahmed Mohammed",
        Role = "student",
        Year = 3,
        Gender = "male",
        PhoneNumber = "+20 1001234567",
        DepartmentId = 1
    },
    // ... more students
    new User 
    { 
        Id = "EMP-00123", 
        UserName = "dr.sarah",
        Email = "dr.sarah@uni.edu",
        DisplayName = "Dr. Sarah Ahmed",
        Role = "instructor",
        DepartmentId = 1
    }
};

// Add courses
var courses = new[]
{
    new Course
    {
        Id = "CS301",
        Code = "CS301",
        Name = "Information Retrieval Systems",
        InstructorId = "EMP-00123",
        Year = 3,
        Credits = 3,
        Level = 3,
        Color = "#818cf8",
        Semester = "Fall 2025"
    },
    // ... more courses
};
```

---

**Last Updated**: 2025
**Target Database**: SQL Server 2019+
