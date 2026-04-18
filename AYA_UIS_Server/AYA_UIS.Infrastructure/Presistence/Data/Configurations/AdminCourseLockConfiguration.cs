using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Presistence.Data.Configurations
{
    public class AdminCourseLockConfiguration : IEntityTypeConfiguration<AdminCourseLock>
    {
        public void Configure(EntityTypeBuilder<AdminCourseLock> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();

            // Use the navigation properties explicitly to prevent EF from creating
            // shadow foreign key properties (UserId1, CourseId1)
            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Course)
                   .WithMany()
                   .HasForeignKey(x => x.CourseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        }
    }
}
