using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Presistence.Data.Configurations
{
    public class RegistrationSettingsConfiguration
        : IEntityTypeConfiguration<RegistrationSettings>
    {
        public void Configure(EntityTypeBuilder<RegistrationSettings> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Semester)
                   .HasMaxLength(50)
                   .HasDefaultValue(string.Empty);

            builder.Property(x => x.AcademicYear)
                   .HasMaxLength(20)
                   .HasDefaultValue(string.Empty);

            builder.Property(x => x.OpenYears)
                   .HasMaxLength(50)
                   .HasDefaultValue(string.Empty);

            // Use MAX to support many course codes
            builder.Property(x => x.EnabledCourses)
                   .HasColumnType("nvarchar(max)")
                   .HasDefaultValue(string.Empty);

            builder.Property(x => x.IsOpen)
                   .HasDefaultValue(false);
        }
    }
}
