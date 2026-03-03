using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Presistence.Data.Configurations
{
    public class StudentCourseExceptionConfiguration
    : IEntityTypeConfiguration<StudentCourseException>
    {
        public void Configure(EntityTypeBuilder<StudentCourseException> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new
            {
                x.UserId,
                x.CourseId,
                x.StudyYearId,
                x.SemesterId
            }).IsUnique();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Course>()
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450); 
        }
    }
}
