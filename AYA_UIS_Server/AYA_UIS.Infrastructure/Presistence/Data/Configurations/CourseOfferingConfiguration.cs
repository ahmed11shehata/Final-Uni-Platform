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
    public class CourseOfferingConfiguration
     : IEntityTypeConfiguration<CourseOffering>
    {
        public void Configure(EntityTypeBuilder<CourseOffering> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new
            {
                x.CourseId,
                x.StudyYearId,
                x.SemesterId,
                x.Level
            }).IsUnique();


            builder.HasOne(x => x.Course)
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne<StudyYear>()
                .WithMany()
                .HasForeignKey(x => x.StudyYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Semester>()
                .WithMany()
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Level)
                   .HasConversion<int>() 
                   .IsRequired();
                  
            builder.Property(x => x.IsOpen)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
}
