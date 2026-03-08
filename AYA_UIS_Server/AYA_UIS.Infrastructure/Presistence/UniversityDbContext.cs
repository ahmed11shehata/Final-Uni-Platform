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

            builder.Entity<User>()
                   .HasIndex(u => u.Academic_Code)
                   .IsUnique();
        }

        // Domain tables
        public DbSet<Department> Departments { get; set; }
        public DbSet<StudyYear> StudyYears { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<AcademicSchedule> AcademicSchedules { get; set; }
        public DbSet<Fee> Fees { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<CourseUpload> CourseUploads { get; set; }
        public DbSet<SemesterGPA> SemesterGPAs { get; set; }
        public DbSet<UserStudyYear> UserStudyYears { get; set; }

        public DbSet<CourseResult> CourseResults { get; set; }
        public DbSet<CourseOffering> CourseOfferings { get; set; }
        public DbSet<StudentCourseException> StudentCourseExceptions { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
    }
}
