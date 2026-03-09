using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Presistence.Data.Configurations
{
    public class StudentQuizAttemptConfiguration : IEntityTypeConfiguration<StudentQuizAttempt>
    {
        public void Configure(EntityTypeBuilder<StudentQuizAttempt> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Score)
                   .IsRequired();

            builder.Property(a => a.SubmittedAt)
                   .IsRequired();

            builder.HasOne(a => a.Quiz)
                   .WithMany()
                   .HasForeignKey(a => a.QuizId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Student)
                   .WithMany()
                   .HasForeignKey(a => a.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
