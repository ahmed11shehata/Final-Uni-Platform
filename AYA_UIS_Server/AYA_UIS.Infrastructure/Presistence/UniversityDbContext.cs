using System.Reflection;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Presistence
{
    public class UniversityDbContext : IdentityDbContext<User>
    {
        public UniversityDbContext(DbContextOptions<UniversityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.Entity<User>().HasIndex(u => u.Academic_Code).IsUnique();
        }

        public DbSet<Department>            Departments            { get; set; }
        public DbSet<StudyYear>             StudyYears             { get; set; }
        public DbSet<Semester>              Semesters              { get; set; }
        public DbSet<AcademicSchedule>      AcademicSchedules      { get; set; }
        public DbSet<Fee>                   Fees                   { get; set; }
        public DbSet<Course>                Courses                { get; set; }
        public DbSet<CoursePrerequisite>    CoursePrerequisites    { get; set; }
        public DbSet<Registration>          Registrations          { get; set; }
        public DbSet<CourseUpload>          CourseUploads          { get; set; }
        public DbSet<SemesterGPA>           SemesterGPAs           { get; set; }
        public DbSet<UserStudyYear>         UserStudyYears         { get; set; }
        public DbSet<CourseResult>          CourseResults          { get; set; }
        public DbSet<CourseOffering>        CourseOfferings        { get; set; }
        public DbSet<StudentCourseException> StudentCourseExceptions { get; set; }
        public DbSet<Assignment>            Assignments            { get; set; }
        public DbSet<AssignmentSubmission>  AssignmentSubmissions  { get; set; }
        public DbSet<Quiz>                  Quizzes                { get; set; }
        public DbSet<QuizQuestion>          QuizQuestions          { get; set; }
        public DbSet<QuizOption>            QuizOptions            { get; set; }
        public DbSet<StudentQuizAttempt>    StudentQuizAttempts    { get; set; }
        public DbSet<StudentAnswer>         StudentAnswers         { get; set; }
        // NEW
        public DbSet<RegistrationSettings>  RegistrationSettings   { get; set; }
        public DbSet<AdminCourseLock>       AdminCourseLocks        { get; set; }
        public DbSet<ScheduleSession>       ScheduleSessions        { get; set; }
        public DbSet<ExamScheduleEntry>     ExamScheduleEntries     { get; set; }
        public DbSet<SchedulePublish>                SchedulePublishes               { get; set; }
        public DbSet<PasswordResetOtp>               PasswordResetOtps               { get; set; }
        public DbSet<RegistrationCourseInstructor>   RegistrationCourseInstructors   { get; set; }
        public DbSet<Notification>                   Notifications                   { get; set; }
        public DbSet<MidtermGrade>                   MidtermGrades                   { get; set; }
        public DbSet<FinalGrade>                     FinalGrades                     { get; set; }
        public DbSet<FinalGradeReview>               FinalGradeReviews               { get; set; }
    }
}
