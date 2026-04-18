using AYA_UIS.Core.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Dtos;
using Shared.Dtos.Auth_Module;
using System;


namespace Shared.Dtos.Auth_Module.UnitTests
{
    /// <summary>
    /// Tests for Shared.Dtos.Auth_Module.FrontendLoginResponseDto
    /// </summary>
    [TestClass]
    public partial class FrontendLoginResponseDtoTests
    {
        /// <summary>
        /// Verifies that FromUserResult maps all fields from a populated UserResultDto to FrontendLoginResponseDto,
        /// including applying string transformations (role -> lower-case, gender -> lower-case, level underscores replaced).
        /// Inputs: a UserResultDto with a mix of nullable and non-nullable values, numeric extremes for numeric fields,
        /// and a null PhoneNumber to test nullable string mapping.
        /// Expected: all corresponding FrontendUserDto properties contain the expected transformed or copied values.
        /// </summary>
        [TestMethod]
        public void FromUserResult_ValidDto_MapsAllFieldsCorrectly()
        {
            // Arrange
            var genders = Enum.GetValues(typeof(Gender));
            var levels = Enum.GetValues(typeof(Levels));

            // pick first enum value deterministically; these calls require the enums to exist in the referenced project
            var sampleGender = (Gender)genders.GetValue(0)!;
            var sampleLevel = (Levels)levels.GetValue(0)!;

            var dto = new UserResultDto
            {
                Id = "user-123",
                DisplayName = "John Doe",
                Email = "John.Doe@Example.COM",
                Token = "secrettoken",
                AcademicCode = "AC-0001",
                PhoneNumber = null, // nullable -> should map through as null
                Role = "ADMIN", // should be converted to lower-case "admin"
                UserName = "johnd",
                TotalCredits = int.MaxValue,
                AllowedCredits = int.MinValue,
                TotalGPA = 3.85m,
                Specialization = "Artificial Intelligence",
                Level = sampleLevel,
                DepartmentName = null,
                DepartmentId = int.MaxValue,
                ProfilePicture = "http://example.com/pic.png",
                Gender = sampleGender,
                CurrentStudyYearId = 0,
                CurrentSemesterId = -1
            };

            var expectedRole = (dto.Role ?? "student").ToLower();
            var expectedGender = dto.Gender.ToString().ToLower();
            var expectedLevel = dto.Level?.ToString()?.Replace("_", " ");

            // Act
            var result = FrontendLoginResponseDto.FromUserResult(dto);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(dto.Token ?? string.Empty, result.Token, "Token should be mapped (null -> empty if applicable).");

            var user = result.User;
            Assert.IsNotNull(user, "User property should be populated.");

            Assert.AreEqual(dto.Id ?? string.Empty, user.Id, "Id should be copied or empty when source is null or empty.");
            Assert.AreEqual(dto.DisplayName ?? string.Empty, user.Name, "DisplayName should be copied to Name.");
            Assert.AreEqual(dto.Email ?? string.Empty, user.Email, "Email should be copied.");
            Assert.AreEqual(expectedRole, user.Role, "Role should be lower-cased and default to 'student' when null.");
            Assert.AreEqual(dto.AcademicCode, user.AcademicCode, "AcademicCode should be copied.");
            Assert.AreEqual(dto.UserName, user.UserName, "UserName should be copied.");

            Assert.AreEqual(expectedGender, user.Gender, "Gender should be the lower-cased enum name.");
            Assert.AreEqual(dto.DepartmentName, user.Department, "DepartmentName should be copied to Department.");
            Assert.AreEqual(dto.DepartmentId, user.DepartmentId, "DepartmentId should be copied.");

            Assert.AreEqual(expectedLevel, user.Level, "Level should be stringified and underscores replaced with spaces when present.");
            Assert.AreEqual(dto.TotalGPA, user.Gpa, "TotalGPA should map to Gpa.");
            Assert.AreEqual(dto.TotalCredits, user.TotalCredits, "TotalCredits should be copied.");
            Assert.AreEqual(dto.AllowedCredits, user.AllowedCredits, "AllowedCredits should be copied.");
            Assert.AreEqual(dto.Specialization, user.Specialization, "Specialization should be copied.");
            Assert.AreEqual(dto.ProfilePicture, user.ProfilePicture, "ProfilePicture should be copied.");

            Assert.AreEqual(dto.PhoneNumber, user.Phone, "PhoneNumber (nullable) should map through as-is (null expected).");
            Assert.AreEqual(dto.CurrentStudyYearId, user.CurrentStudyYearId, "CurrentStudyYearId should be copied.");
            Assert.AreEqual(dto.CurrentSemesterId, user.CurrentSemesterId, "CurrentSemesterId should be copied.");
        }

    }
}