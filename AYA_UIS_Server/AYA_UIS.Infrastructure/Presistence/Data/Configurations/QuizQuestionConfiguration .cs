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
    public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
    {
        public void Configure(EntityTypeBuilder<QuizQuestion> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.Type)
       .IsRequired();

            builder.Property(q => q.QuestionText)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.HasOne(q => q.Quiz)
                   .WithMany(q => q.Questions)
                   .HasForeignKey(q => q.QuizId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(q => q.Type)
       .IsRequired();
        }
    }
}
