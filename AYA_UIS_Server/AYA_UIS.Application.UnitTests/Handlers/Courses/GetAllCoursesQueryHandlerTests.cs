using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Courses;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Handlers.Courses.UnitTests
{
    [TestClass]
    public class GetAllCoursesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null instance when provided a valid IUnitOfWork implementation.
        /// Input conditions: a non-null, mocked IUnitOfWork is provided.
        /// Expected result: an instance of GetAllCoursesQueryHandler is created and implements the expected IRequestHandler interface; no exception thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_CreatesInstance()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler = new GetAllCoursesQueryHandler(unitOfWorkMock.Object);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetAllCoursesQuery, IEnumerable<FrontendCourseDto>>));
        }

        /// <summary>
        /// Ensures the constructor does not call into the provided IUnitOfWork during construction.
        /// Input conditions: a Strict Moq IUnitOfWork mock with no setups.
        /// Expected result: construction succeeds without triggering mock invocations (which would throw for Strict behavior).
        /// </summary>
        [TestMethod]
        public void Constructor_DoesNotInvokeUnitOfWorkOnConstruction()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            // Intentionally do not setup any members to ensure constructor does not call any.

            // Act
            var handler = new GetAllCoursesQueryHandler(unitOfWorkMock.Object);

            // Assert
            // If constructor invoked any IUnitOfWork member, the Strict mock would have thrown.
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Verifies that Handle maps a set of Course entities to FrontendCourseDto correctly.
        /// Inputs: a mixture of course codes (empty, no valid year digit, valid year digit), various Credits (including int.MaxValue),
        /// Department with empty name (to produce empty Dept) and non-empty name, and different CourseStatus values.
        /// Expected: Returned sequence preserves count and maps fields (Id, Code, Name, Credits, Year, Semester, Type, Status, Dept, Prereqs, Color, RegStatus, IsPassed)
        /// and color selection cycles using the handler's internal palette.
        /// </summary>
        [TestMethod]
        public async Task Handle_MultipleCourses_MapsToFrontendCourseDtos_WithExpectedValues()
        {
            // Arrange
            var colors = new[] { "#6366f1", "#8b5cf6", "#0ea5e9", "#f59e0b", "#ef4444", "#14b8a6", "#e05c8a", "#22c55e" };

            var courses = new List<Course>
            {
                new Course { Id = 1, Code = "", Name = "EmptyCodeCourse", Credits = 3, Status = CourseStatus.Opened, Department = new Department { Name = "CompSci" } },
                new Course { Id = 2, Code = "AB5", Name = "NoValidYearDigit", Credits = 4, Status = CourseStatus.Closed, Department = new Department { Name = string.Empty } },
                new Course { Id = 3, Code = "CS2012", Name = "Year2Course", Credits = 5, Status = CourseStatus.Opened, Department = new Department { Name = "EEE" } },
                new Course { Id = 4, Code = "H1051", Name = "Year1Course", Credits = 6, Status = CourseStatus.Closed, Department = new Department { Name = "HUM" } },
                new Course { Id = 5, Code = "X345", Name = "Year3Course", Credits = 7, Status = CourseStatus.Opened, Department = new Department { Name = "SCI" } },
                new Course { Id = 6, Code = "Z444", Name = "Year4Course", Credits = 8, Status = CourseStatus.Closed, Department = new Department { Name = "MATH" } },
                new Course { Id = 7, Code = "A1B2", Name = "FirstDigit1", Credits = 9, Status = CourseStatus.Opened, Department = new Department { Name = "LAW" } },
                new Course { Id = 8, Code = "B2C3", Name = "FirstDigit2", Credits = 10, Status = CourseStatus.Closed, Department = new Department { Name = "MED" } },
                // Ninth item to validate color cycling (index 8 -> colors[8 % 8] == colors[0])
                new Course { Id = 9, Code = "CS3012", Name = "WrapColorCourse", Credits = int.MaxValue, Status = CourseStatus.Opened, Department = new Department { Name = "COMP" } }
            }.AsEnumerable();

            var repoMock = new Mock<ICourseRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(courses);

            var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            uowMock.Setup(u => u.Courses).Returns(repoMock.Object);

            var handler = new GetAllCoursesQueryHandler(uowMock.Object);

            // Act
            var result = await handler.Handle(new GetAllCoursesQuery(), CancellationToken.None);
            var list = result.ToList();

            // Assert
            Assert.AreEqual(9, list.Count, "Expected the same number of DTOs as source courses.");

            // Verify mapping for several representative items
            Assert.AreEqual(1, list[0].Id);
            Assert.AreEqual("", list[0].Code);
            Assert.AreEqual("EmptyCodeCourse", list[0].Name);
            Assert.AreEqual(3, list[0].Credits);
            // empty code -> Year should default to 1
            Assert.AreEqual(1, list[0].Year);
            Assert.AreEqual(1, list[0].Semester);
            Assert.AreEqual("mandatory", list[0].Type);
            Assert.AreEqual(CourseStatus.Opened.ToString(), list[0].Status);
            Assert.AreEqual("CompSci", list[0].Dept);
            CollectionAssert.AreEqual(Array.Empty<string>(), list[0].Prereqs);
            Assert.AreEqual(colors[0], list[0].Color);
            Assert.IsNull(list[0].Instructor);
            Assert.IsNull(list[0].Description);
            Assert.AreEqual("available", list[0].RegStatus);
            Assert.IsNull(list[0].Grade);
            Assert.IsFalse(list[0].IsPassed);

            // Case: department with empty name produces empty Dept
            Assert.AreEqual(string.Empty, list[1].Dept);

            // Year extraction cases
            Assert.AreEqual(2, list[2].Year, "CS2012 should produce year 2.");
            Assert.AreEqual(1, list[3].Year, "H1051 should produce year 1 (first allowed digit is '1').");
            Assert.AreEqual(3, list[4].Year, "X345 should produce year 3 (first allowed digit '3').");
            Assert.AreEqual(4, list[5].Year, "Z444 should produce year 4 (first allowed digit '4').");
            Assert.AreEqual(1, list[1].Year, "AB5 has no 1-4 digit and should default to 1.");

            // Credits edge case (int.MaxValue)
            Assert.AreEqual(int.MaxValue, list[8].Credits);

            // Color cycle: index 8 should map to colors[8 % colors.Length] == colors[0]
            Assert.AreEqual(colors[8 % colors.Length], list[8].Color);

            // Verify that repository method was invoked
            repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            uowMock.Verify(u => u.Courses, Times.AtLeastOnce);
        }

        /// <summary>
        /// Ensures that when repository returns an empty collection, Handle returns an empty enumerable without throwing.
        /// Inputs: repository returns empty sequence.
        /// Expected: result is empty and no exception thrown.
        /// </summary>
        [TestMethod]
        public async Task Handle_EmptyCourses_ReturnsEmptyEnumerable()
        {
            // Arrange
            var repoMock = new Mock<ICourseRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(Enumerable.Empty<Course>());

            var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            uowMock.Setup(u => u.Courses).Returns(repoMock.Object);

            var handler = new GetAllCoursesQueryHandler(uowMock.Object);

            // Act
            var result = await handler.Handle(new GetAllCoursesQuery(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count(), "Expected no DTOs when source has no courses.");

            repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            uowMock.Verify(u => u.Courses, Times.AtLeastOnce);
        }

        /// <summary>
        /// Verifies ExtractYear returns expected year for a variety of representative inputs.
        /// Inputs include typical course codes, codes with digits outside the 1-4 range,
        /// codes with the target digit appearing later, and codes with no qualifying digits.
        /// Expected: numeric year 1..4 when a qualifying digit is found; otherwise 1.
        /// </summary>
        [TestMethod]
        public void ExtractYear_VariousInputs_ReturnsExpected()
        {
            // Arrange
            var cases = new (string Input, int Expected)[]
            {
                ("CS1011", 1),            // first digit '1' -> 1
                ("CS2012", 2),            // first digit '2' -> 2
                ("H1051", 1),             // first digit '1' -> 1
                ("AB5C3", 3),             // '5' ignored, '3' returns 3
                ("0ABC4", 4),             // '0' ignored, '4' returns 4
                ("XYZ", 1),               // no digits -> default 1
                ("1234", 1),              // first char '1' -> 1
                ("ABCDE5FG", 1),          // only digit 5 (out of range) -> 1
                ("05679", 1),             // digits present but none in 1..4 -> 1
                ("\t\n1\r", 1)            // control characters with '1' -> 1
            };

            // Act & Assert
            foreach (var (input, expected) in cases)
            {
                int actual = GetAllCoursesQueryHandler.ExtractYear(input);
                Assert.AreEqual(expected, actual, $"Input '{input}' should yield {expected} but returned {actual}.");
            }
        }

        /// <summary>
        /// Tests edge and special-case string inputs:
        /// - empty string and whitespace-only strings should return 1 (default)
        /// - very long strings with a qualifying digit near the end should still return the correct year
        /// - non-ASCII digit characters are ignored by the ASCII range check; subsequent ASCII digit is used
        /// </summary>
        [TestMethod]
        public void ExtractYear_EdgeCases_WhitespaceLongAndNonAsciiHandledCorrectly()
        {
            // Arrange - whitespace and empty
            var whitespaceCases = new[] { string.Empty, "   ", "\t", "\n\r" };

            foreach (var ws in whitespaceCases)
            {
                // Act
                int actual = GetAllCoursesQueryHandler.ExtractYear(ws);

                // Assert
                Assert.AreEqual(1, actual, $"Whitespace/empty input '{ws}' should return default year 1.");
            }

            // Arrange - very long string with qualifying digit near the end
            string longPrefix = new string('A', 2000);
            string longInput = longPrefix + "4XYZ"; // '4' near the end
            int longActual = GetAllCoursesQueryHandler.ExtractYear(longInput);
            Assert.AreEqual(4, longActual, "Very long string should locate '4' and return 4.");

            // Arrange - non-ASCII digit (Arabic-Indic U+0662) followed by ASCII '3'
            string nonAsciiThenAscii = "\u06623"; // U+0662 (digit 2, ignored by ASCII range) then '3'
            int nonAsciiActual = GetAllCoursesQueryHandler.ExtractYear(nonAsciiThenAscii);

            // Assert: ASCII '3' should be recognized and returned as 3
            Assert.AreEqual(3, nonAsciiActual, "Non-ASCII digit should be ignored; subsequent ASCII '3' should return 3.");
        }
    }
}