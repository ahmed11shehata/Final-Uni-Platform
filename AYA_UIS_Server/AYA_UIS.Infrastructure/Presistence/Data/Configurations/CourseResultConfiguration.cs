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
    public class CourseResultConfiguration : IEntityTypeConfiguration<CourseResult>
    {
        public void Configure(EntityTypeBuilder<CourseResult> builder)
        {
            builder.HasKey(x => x.Id);

            // مادة واحدة فقط في سنة دراسية واحدة
            builder.HasIndex(x => new { x.UserId, x.CourseId, x.StudyYearId })
                   .IsUnique();

            builder.Property(x => x.Grade)
                   .HasPrecision(5, 2);

            builder.HasOne(x => x.Course)
                   .WithMany()
                   .HasForeignKey(x => x.CourseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.StudyYear)
                   .WithMany()
                   .HasForeignKey(x => x.StudyYearId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
