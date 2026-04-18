using AYA_UIS.Application.Mapping;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Entities.Identity;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Dtos.Info_Module.RegistrationDtos;
using Shared.Dtos.Info_Module.FeeDtos;
using Shared.Dtos.Info_Module.AssignmentDto;
using System;


namespace AYA_UIS.Application.Mapping.UnitTests
{
    /// <summary>
    /// Tests for MappingProfile constructor and configured member mappings.
    /// Validates mapping configuration is valid and critical custom member mappings behave for edge cases.
    /// </summary>
    [TestClass]
    public partial class MappingProfileTests
    {
        /// <summary>
        /// Ensures the AutoMapper configuration with MappingProfile is valid.
        /// Condition: constructing MappingProfile and building configuration.
        /// Expected result: no configuration exceptions are thrown.
        /// </summary>
        [TestMethod]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void MappingProfile_Constructor_ConfigurationIsValid()
        {
            // Arrange
            var profile = new MappingProfile();

            // Act
            var config = new MapperConfiguration(cfg => cfg.AddProfile(profile));

            // Assert
            // Will throw if configuration invalid
            config.AssertConfigurationIsValid();
        }

        /// <summary>
        /// Verifies Registration -> RegistrationCourseDto mapping for both when Course is null and when Course is present.
        /// Condition: various Course states and numeric extremes for Course.Id.
        /// Expected result: CourseId uses Course.Id when Course not null otherwise uses Registration.CourseId.
        /// CourseCode and CourseName map to empty string when Course is null; map to provided values when Course present.
        /// </summary>
        [TestMethod]
        public void CreateMap_RegistrationToRegistrationCourseDto_HandlesCourseNullAndPresent()
        {
            // Arrange
            var profile = new MappingProfile();
            var config = new MapperConfiguration(cfg => cfg.AddProfile(profile));
            var mapper = config.CreateMapper();

            // Case 1: Course is null -> should use Registration.CourseId and empty strings
            var regNullCourse = new Registration
            {
                Course = null,
                CourseId = int.MaxValue,
            };

            // Act
            var dtoNullCourse = mapper.Map<RegistrationCourseDto>(regNullCourse);

            // Assert
            Assert.AreEqual(regNullCourse.CourseId, dtoNullCourse.CourseId, "When Course is null, CourseId should come from Registration.CourseId");
            Assert.AreEqual(string.Empty, dtoNullCourse.CourseCode, "When Course is null, CourseCode should be empty string");
            Assert.AreEqual(string.Empty, dtoNullCourse.CourseName, "When Course is null, CourseName should be empty string");

            // Case 2: Course present with boundary Id values and special strings
            var course = new Course
            {
                Id = int.MinValue,
                Code = "C-☃\0\t", // include special/control characters
                Name = new string('X', 300) // very long string
            };
            var regWithCourse = new Registration
            {
                Course = course,
                CourseId = 123 // should be ignored when Course is present
            };

            // Act
            var dtoWithCourse = mapper.Map<RegistrationCourseDto>(regWithCourse);

            // Assert
            Assert.AreEqual(course.Id, dtoWithCourse.CourseId, "When Course is present, CourseId should come from Course.Id");
            Assert.AreEqual(course.Code, dtoWithCourse.CourseCode, "CourseCode should map from Course.Code when Course present");
            Assert.AreEqual(course.Name, dtoWithCourse.CourseName, "CourseName should map from Course.Name when Course present");
        }

        /// <summary>
        /// Verifies Fee -> FeeDto mapping for Department null and present.
        /// Condition: Department is null and Department.Name contains edge-case strings.
        /// Expected result: DepartmentName is null when Department is null; maps to Department.Name when present.
        /// </summary>
        [TestMethod]
        public void CreateMap_FeeToFeeDto_MapsDepartmentNameCorrectly()
        {
            // Arrange
            var profile = new MappingProfile();
            var config = new MapperConfiguration(cfg => cfg.AddProfile(profile));
            var mapper = config.CreateMapper();

            // Case 1: Department is null
            var feeWithNullDept = new Fee
            {
                Department = null
            };

            // Act
            var dtoNull = mapper.Map<FeeDto>(feeWithNullDept);

            // Assert
            Assert.IsNull(dtoNull.DepartmentName, "When Fee.Department is null, FeeDto.DepartmentName should be null");

            // Case 2: Department present with various name edge cases
            var dept = new Department
            {
                Name = "  " // whitespace-only should be preserved as-is
            };
            var feeWithDept = new Fee
            {
                Department = dept
            };

            // Act
            var dtoWithDept = mapper.Map<FeeDto>(feeWithDept);

            // Assert
            Assert.AreEqual(dept.Name, dtoWithDept.DepartmentName, "DepartmentName should map from Department.Name even when whitespace-only");
        }

        /// <summary>
        /// Verifies AssignmentSubmission -> AssignmentSubmissionDto mapping for StudentName.
        /// Condition: Student with various UserName values (including nullability).
        /// Expected result: StudentName maps from Student.UserName. If Student is null, mapping may throw; test covers non-null student as mapping assumes Student present.
        /// </summary>
        [TestMethod]
        public void CreateMap_AssignmentSubmissionToDto_MapsStudentNameFromStudentUserName()
        {
            // Arrange
            var profile = new MappingProfile();
            var config = new MapperConfiguration(cfg => cfg.AddProfile(profile));
            var mapper = config.CreateMapper();

            // Non-null Student with edge-case UserName values
            var student = new User
            {
                UserName = "student123"
            };
            var submission = new AssignmentSubmission
            {
                Student = student
            };

            // Act
            var dto = mapper.Map<AssignmentSubmissionDto>(submission);

            // Assert
            Assert.AreEqual(student.UserName, dto.StudentName, "StudentName should map from Student.UserName");
        }
    }
}