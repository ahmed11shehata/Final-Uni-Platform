using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Presistence.Data.Configurations
{
    internal class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.Title)
                   .IsRequired()
                   .HasMaxLength(200);


            builder.Property(q => q.StartTime)
                   .IsRequired();

            builder.Property(q => q.EndTime)
                   .IsRequired();



            builder.HasOne(q => q.Course)
                   .WithMany()
                   .HasForeignKey(q => q.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
