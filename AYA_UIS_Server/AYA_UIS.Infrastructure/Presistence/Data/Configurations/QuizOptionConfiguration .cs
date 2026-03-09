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
    public class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
    {
        public void Configure(EntityTypeBuilder<QuizOption> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Text)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(o => o.IsCorrect)
                   .IsRequired();

            builder.HasOne(o => o.Question)
                   .WithMany(q => q.Options)
                   .HasForeignKey(o => o.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
