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
    public class StudentAnswerConfiguration : IEntityTypeConfiguration<StudentAnswer>
    {
        public void Configure(EntityTypeBuilder<StudentAnswer> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.Question)
                   .WithMany()
                   .HasForeignKey(a => a.QuestionId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.SelectedOption)
                   .WithMany()
                   .HasForeignKey(a => a.SelectedOptionId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Attempt)
                   .WithMany(a => a.Answers)
                   .HasForeignKey(a => a.AttemptId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
