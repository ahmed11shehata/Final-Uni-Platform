using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Presistence.Data.Configurations
{
    public class RegistrationCourseInstructorConfiguration : IEntityTypeConfiguration<RegistrationCourseInstructor>
    {
        public void Configure(EntityTypeBuilder<RegistrationCourseInstructor> builder)
        {
            builder.HasKey(x => x.Id);

            // Each (Course, Instructor) pair must be unique
            builder.HasIndex(x => new { x.CourseId, x.InstructorId }).IsUnique();

            builder.HasOne(x => x.Course)
                   .WithMany()
                   .HasForeignKey(x => x.CourseId)
                   .OnDelete(DeleteBehavior.Restrict);

            // When the instructor account is deleted, remove their assignments automatically
            builder.HasOne(x => x.Instructor)
                   .WithMany()
                   .HasForeignKey(x => x.InstructorId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.InstructorId).IsRequired().HasMaxLength(450);
        }
    }
}
