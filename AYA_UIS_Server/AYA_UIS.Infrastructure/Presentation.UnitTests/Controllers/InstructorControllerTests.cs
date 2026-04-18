using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos;
using Shared.Dtos.Instructor_Module;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public partial class InstructorControllerTests
    {
        /// <summary>
        /// Purpose: Verify that GetCourseStudents returns an OkObjectResult containing an empty list of StudentInCourseDto
        /// when invoked with a variety of valid (non-null) courseId inputs.
        /// Inputs tested: empty string, whitespace-only string, very long string (10,000 chars),
        /// typical identifier, and string with special/control characters.
        /// Expected result: method does not throw, returns ActionResult with Result as OkObjectResult,
        /// OkObjectResult.Value is a non-null List&lt;StudentInCourseDto&gt; with Count == 0.
        /// </summary>
        [TestMethod]
        public async Task GetCourseStudents_VariousCourseIdInputs_ReturnsEmptyOkList()
        {
            // Arrange
            var controller = new InstructorController();

            var testInputs = new[]
            {
                string.Empty,
                "   ",
                new string('a', 10_000),
                "course-123",
                "special-!@#$%^&*()\t\n"
            };

            foreach (string courseId in testInputs)
            {
                // Act
                var actionResult = await controller.GetCourseStudents(courseId);

                // Assert - top-level ActionResult should be present
                Assert.IsNotNull(actionResult, "ActionResult should not be null.");

                // When returning Ok(...), ActionResult<T>.Value is expected to be null and Result to be OkObjectResult
                Assert.IsNull(actionResult.Value, "ActionResult.Value should be null when an ActionResult (Ok) is returned.");

                // Verify Result is OkObjectResult and contains an empty List<StudentInCourseDto>
                Assert.IsNotNull(actionResult.Result, "ActionResult.Result should not be null.");
                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), "Result should be OkObjectResult.");

                var okResult = (OkObjectResult)actionResult.Result!;
                Assert.IsNotNull(okResult.Value, "OkObjectResult.Value should not be null.");
                Assert.IsInstanceOfType(okResult.Value, typeof(List<StudentInCourseDto>), "OkObjectResult.Value should be a List<StudentInCourseDto>.");

                var list = (List<StudentInCourseDto>)okResult.Value!;
                Assert.AreEqual(0, list.Count, "Returned list should be empty.");
            }
        }

        /// <summary>
        /// Verifies that GetAssignments returns an OkObjectResult containing an empty list
        /// for a variety of courseId inputs (null, empty, whitespace, typical, very long, special chars).
        /// Expected: method completes successfully and response is Ok with an empty List&lt;InstructorAssignmentDto&gt;.
        /// </summary>
        [TestMethod]
        public async Task GetAssignments_VariousCourseIdValues_ReturnsOkWithEmptyList()
        {
            // Arrange
            var controller = new Presentation.Controllers.InstructorController();

            // Prepare diverse test inputs for the nullable courseId parameter
            string?[] testInputs = new string?[]
            {
                null,                       // nullable case
                string.Empty,               // empty string
                "   ",                      // whitespace-only
                "CS101",                    // typical course id
                new string('a', 10000),     // very long string
                "special-çö∑®\t\n\r\b"      // special and control characters
            };

            foreach (string? courseId in testInputs)
            {
                // Act
                Task<ActionResult<List<InstructorAssignmentDto>>> taskResult = controller.GetAssignments(courseId);
                ActionResult<List<InstructorAssignmentDto>> actionResult = await taskResult.ConfigureAwait(false);

                // Assert: top-level ActionResult has a Result set to OkObjectResult
                Assert.IsNotNull(actionResult);
                Assert.IsNotNull(actionResult.Result, "Expected Result to be set (OkObjectResult) when controller returns Ok(...)");

                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), "Expected OkObjectResult for various courseId inputs");

                var okResult = (OkObjectResult)actionResult.Result!;
                Assert.IsNotNull(okResult.Value, "OkObjectResult.Value should not be null and should be a List<InstructorAssignmentDto>");
                Assert.IsInstanceOfType(okResult.Value, typeof(List<InstructorAssignmentDto>), "OkObjectResult.Value should be List<InstructorAssignmentDto>");

                var list = (List<InstructorAssignmentDto>)okResult.Value!;
                Assert.AreEqual(0, list.Count, "Returned list should be empty as per current implementation");
            }
        }

        /// <summary>
        /// Ensures that GetAssignments completes synchronously/quickly and does not throw for a typical input.
        /// Input: courseId = \"test\". Expected: no exceptions and OkObjectResult returned.
        /// This test emphasizes that the async Task completes successfully.
        /// </summary>
        [TestMethod]
        public async Task GetAssignments_TypicalCourseId_TaskCompletesSuccessfully()
        {
            // Arrange
            var controller = new Presentation.Controllers.InstructorController();
            string courseId = "test";

            // Act & Assert (no exception expected)
            Task<ActionResult<List<InstructorAssignmentDto>>> task = controller.GetAssignments(courseId);
            ActionResult<List<InstructorAssignmentDto>> result = await task.ConfigureAwait(false);

            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Verifies that CreateAssignment returns a CreatedAtActionResult containing an InstructorAssignmentDto
        /// for a variety of valid CreateInstructorAssignmentDto inputs (including edge-like values).
        /// Inputs tested:
        /// - Empty/default strings and empty list
        /// - Whitespace-only strings
        /// - Very long strings
        /// - Strings containing control/special characters
        /// - Numeric edge values for MaxGrade (0, int.MaxValue, int.MinValue)
        /// Expected:
        /// - No exception is thrown.
        /// - The ActionResult.Result is a CreatedAtActionResult.
        /// - The CreatedAtActionResult.StatusCode equals 201.
        /// - The CreatedAtActionResult.ActionName equals nameof(InstructorController.GetAssignments).
        /// - The CreatedAtActionResult.Value is an instance of InstructorAssignmentDto (non-null).
        /// </summary>
        [TestMethod]
        public async Task CreateAssignment_VariousValidDtos_ReturnsCreatedAtActionWithValue()
        {
            // Arrange
            var controller = new InstructorController();

            // Prepare a set of diverse DTOs to exercise edge-like values.
            var veryLongString = new string('a', 10000);
            var controlChars = "Title\u0000\u001F\n\r!@#€";

            var testCases = new List<CreateInstructorAssignmentDto>
            {
                // Default / empty values
                new CreateInstructorAssignmentDto
                {
                    Title = string.Empty,
                    Description = string.Empty,
                    CourseCode = string.Empty,
                    Deadline = string.Empty,
                    MaxGrade = 0,
                    AllowedFormats = new List<string>()
                },

                // Whitespace-only strings
                new CreateInstructorAssignmentDto
                {
                    Title = "   ",
                    Description = " \t ",
                    CourseCode = "   ",
                    Deadline = " ",
                    MaxGrade = 1,
                    AllowedFormats = new List<string> { "pdf" }
                },

                // Very long strings
                new CreateInstructorAssignmentDto
                {
                    Title = veryLongString,
                    Description = veryLongString,
                    CourseCode = veryLongString,
                    Deadline = veryLongString,
                    MaxGrade = int.MaxValue,
                    AllowedFormats = new List<string> { veryLongString }
                },

                // Control / special characters and negative boundary value
                new CreateInstructorAssignmentDto
                {
                    Title = controlChars,
                    Description = controlChars,
                    CourseCode = "C-¥-§",
                    Deadline = "1970-01-01T00:00:00Z",
                    MaxGrade = int.MinValue,
                    AllowedFormats = new List<string> { "zip", "tar.gz", controlChars }
                }
            };

            foreach (var dto in testCases)
            {
                // Act
                ActionResult<InstructorAssignmentDto> actionResult = null!;
                Exception? caught = null;
                try
                {
                    actionResult = await controller.CreateAssignment(dto);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }

                // Assert - no exception thrown
                Assert.IsNull(caught, $"Expected no exception for DTO with Title '{dto.Title}', but got: {caught}");

                // The framework returns an ActionResult<T> whose Result should be a CreatedAtActionResult
                var createdResult = actionResult.Result as CreatedAtActionResult;
                Assert.IsNotNull(createdResult, "Expected Result to be CreatedAtActionResult.");

                // Status code should be 201
                Assert.AreEqual(201, createdResult!.StatusCode, "Expected HTTP status code 201 (Created).");

                // Action name should point to GetAssignments
                Assert.AreEqual(nameof(InstructorController.GetAssignments), createdResult.ActionName, "CreatedAtAction should reference GetAssignments action.");

                // The returned value should be an InstructorAssignmentDto
                Assert.IsNotNull(createdResult.Value, "Expected CreatedAtActionResult.Value to be non-null.");
                Assert.IsInstanceOfType(createdResult.Value, typeof(InstructorAssignmentDto), "Expected CreatedAtActionResult.Value to be of type InstructorAssignmentDto.");
            }
        }

        /// <summary>
        /// Ensures that CreateAssignment produces a CreatedAtActionResult with a non-null value type
        /// specifically when AllowedFormats contains duplicates and single-item lists.
        /// Input conditions:
        /// - AllowedFormats single item, and duplicated formats.
        /// Expected:
        /// - CreatedAtActionResult returned and Value is InstructorAssignmentDto.
        /// This test targets collection-related edge-cases on the DTO.
        /// </summary>
        [TestMethod]
        public async Task CreateAssignment_CollectionEdgeCases_ReturnsCreatedAtActionWithValue()
        {
            // Arrange
            var controller = new InstructorController();

            var singleItemFormats = new CreateInstructorAssignmentDto
            {
                Title = "SingleFormat",
                Description = "Desc",
                CourseCode = "CS101",
                Deadline = "2026-12-31",
                MaxGrade = 100,
                AllowedFormats = new List<string> { "pdf" }
            };

            var duplicateFormats = new CreateInstructorAssignmentDto
            {
                Title = "DuplicateFormats",
                Description = "Desc",
                CourseCode = "CS102",
                Deadline = "2026-12-31",
                MaxGrade = 50,
                AllowedFormats = new List<string> { "pdf", "pdf", "zip" }
            };

            var cases = new[] { singleItemFormats, duplicateFormats };

            foreach (var dto in cases)
            {
                // Act
                var actionResult = await controller.CreateAssignment(dto);

                // Assert
                var createdResult = actionResult.Result as CreatedAtActionResult;
                Assert.IsNotNull(createdResult, "Expected CreatedAtActionResult for collection edge-case DTO.");
                Assert.AreEqual(201, createdResult!.StatusCode, "Expected 201 Created for collection edge-case DTO.");
                Assert.IsInstanceOfType(createdResult.Value, typeof(InstructorAssignmentDto), "Expected returned value to be InstructorAssignmentDto.");
            }
        }

        /// <summary>
        /// Purpose: Verify that GetCourseMaterials returns an OkObjectResult containing an empty list for a variety of valid (non-null) courseId inputs.
        /// Conditions: Tests include typical id, empty string, whitespace-only, very long string, and special/control characters.
        /// Expected: Method does not throw; the ActionResult.Result is an OkObjectResult, StatusCode == 200, and the contained List&lt;InstructorMaterialDto&gt; is non-null and empty.
        /// </summary>
        [TestMethod]
        public async Task GetCourseMaterials_VariousNonNullCourseIds_ReturnsOkWithEmptyList()
        {
            // Arrange
            var controller = new InstructorController();

            var testInputs = new List<string>
            {
                "course-123",                  // typical id
                string.Empty,                  // empty string
                "   ",                         // whitespace-only
                new string('a', 10000),        // very long string
                "special/\0\r\n?%*chars"       // special/control characters
            };

            // Act & Assert - iterate inputs to avoid redundant test methods
            foreach (string courseId in testInputs)
            {
                // Act
                ActionResult<List<InstructorMaterialDto>> actionResult = await controller.GetCourseMaterials(courseId);

                // Assert - wrapper not null
                Assert.IsNotNull(actionResult, "Returned ActionResult should not be null.");

                // Assert - result contains an OkObjectResult
                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), $"Expected OkObjectResult for courseId='{courseId}'.");

                var okResult = (OkObjectResult)actionResult.Result!;

                // Assert - status code 200
                Assert.AreEqual(200, okResult.StatusCode, $"Expected status code 200 for courseId='{courseId}'.");

                // Assert - value is a list of InstructorMaterialDto and is empty
                Assert.IsInstanceOfType(okResult.Value, typeof(List<InstructorMaterialDto>), $"Expected value to be List<InstructorMaterialDto> for courseId='{courseId}'.");
                var list = (List<InstructorMaterialDto>)okResult.Value!;
                Assert.AreEqual(0, list.Count, $"Expected empty list for courseId='{courseId}'.");
            }
        }

        /// <summary>
        /// Purpose: Ensure ActionResult.Value (the generic value) remains null when the controller returns an OkObjectResult and the payload is provided via Result.
        /// Conditions: Single normal courseId input.
        /// Expected: actionResult.Value is null and actionResult.Result is OkObjectResult.
        /// </summary>
        [TestMethod]
        public async Task GetCourseMaterials_WhenReturningOk_ResultProvidedInResult_NotInValue()
        {
            // Arrange
            var controller = new InstructorController();
            string courseId = "course-1";

            // Act
            ActionResult<List<InstructorMaterialDto>> actionResult = await controller.GetCourseMaterials(courseId);

            // Assert
            // When ActionResult<T> is produced via Ok(object) the Value property is not populated (remains default).
            Assert.IsNull(actionResult.Value, "Expected generic Value to be null when OkObjectResult is used.");
            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), "Expected Result to be OkObjectResult.");
        }

        /// <summary>
        /// Tests that DeleteAssignment returns an OkObjectResult containing an anonymous object
        /// with a 'message' property equal to "Assignment deleted successfully" for a variety
        /// of assignmentId inputs, including empty, whitespace-only, very long, and special-character strings.
        /// Input conditions: assignmentId is non-null (method signature is non-nullable) and varies over edge cases.
        /// Expected result: method completes without exception, returns OkObjectResult with a 'message' property
        /// whose value equals "Assignment deleted successfully".
        /// </summary>
        [TestMethod]
        public async Task DeleteAssignment_VariousValidAndEdgeAssignmentIds_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var controller = new InstructorController();

            // Several string edge-cases for the non-nullable assignmentId parameter:
            // - normal simple id
            // - empty string
            // - whitespace-only string
            // - very long string (1KB)
            // - string with special/control characters (including null char and newlines)
            var testInputs = new List<string>
            {
                "normal-id-123",
                string.Empty,
                "   ",
                new string('a', 1024),
                "special-!@#\0\r\n"
            };

            foreach (var assignmentId in testInputs)
            {
                // Act
                var actionResult = await controller.DeleteAssignment(assignmentId);

                // Assert - basic non-null and correct result type
                Assert.IsNotNull(actionResult, $"Expected a non-null result for assignmentId='{assignmentId}'");
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult, $"Expected OkObjectResult for assignmentId='{assignmentId}'");

                // Assert - status code is 200 (OK)
                // OkObjectResult.StatusCode should be 200 for Ok responses
                Assert.AreEqual(200, okResult.StatusCode, $"Expected StatusCode 200 for assignmentId='{assignmentId}'");

                // Assert - returned anonymous object contains 'message' property with expected value
                Assert.IsNotNull(okResult.Value, $"Expected non-null Value in OkObjectResult for assignmentId='{assignmentId}'");
                var value = okResult.Value;
                var messageProperty = value.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(messageProperty, $"Expected returned object to have a 'message' property for assignmentId='{assignmentId}'");

                var messageValue = messageProperty.GetValue(value) as string;
                Assert.AreEqual("Assignment deleted successfully", messageValue, $"Unexpected message for assignmentId='{assignmentId}'");
            }
        }

        /// <summary>
        /// Ensures that DeleteAssignment does not throw for a large-but-feasible assignmentId (boundary stress).
        /// Purpose: validate that the method handles a large string without throwing exceptions.
        /// Input conditions: very large assignmentId (~10k characters).
        /// Expected result: method completes and returns OkObjectResult with the expected message.
        /// </summary>
        [TestMethod]
        public async Task DeleteAssignment_VeryLargeAssignmentId_DoesNotThrowAndReturnsOk()
        {
            // Arrange
            var controller = new InstructorController();
            var veryLargeId = new string('x', 10_000); // large but feasible string size for unit test

            // Act
            var result = await controller.DeleteAssignment(veryLargeId);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult for very large assignmentId");

            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);
            var prop = value.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(prop);
            var message = prop.GetValue(value) as string;
            Assert.AreEqual("Assignment deleted successfully", message);
        }

        /// <summary>
        /// Verifies that UploadMaterial returns an OkObjectResult containing an InstructorMaterialDto
        /// for a variety of courseId and CreateMaterialDto inputs.
        /// Inputs tested: empty string, whitespace-only, very long string, and strings with special/control characters.
        /// Expected: method completes successfully, result.Result is OkObjectResult, and the value is an InstructorMaterialDto
        /// with its default-initialized properties (empty strings and Downloads == 0).
        /// </summary>
        [TestMethod]
        public async Task UploadMaterial_VariousCourseIdsAndDtos_ReturnsOkWithDefaultInstructorMaterialDto()
        {
            // Arrange
            var controller = new InstructorController();

            var courseIds = new[]
            {
                string.Empty,
                "   ",
                new string('a', 1024), // very long
                "special-!@#\t\n\r"
            };

            var dtos = new[]
            {
                new CreateMaterialDto(), // defaults (empty strings)
                new CreateMaterialDto
                {
                    Title = new string('T', 512),
                    Description = "Desc with special \u0001\u0002 chars",
                    Type = "application/pdf"
                }
            };

            // Act & Assert - iterate combinations to cover parameter permutations concisely
            foreach (var courseId in courseIds)
            {
                foreach (var dto in dtos)
                {
                    // Act
                    var actionResult = await controller.UploadMaterial(courseId, dto);

                    // Assert - top-level not null
                    Assert.IsNotNull(actionResult);

                    // Assert - the ActionResult should be an OkObjectResult
                    Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), $"Expected OkObjectResult for courseId '{TruncateForAssertion(courseId)}' and dto.Title '{TruncateForAssertion(dto.Title)}'.");

                    var okResult = (OkObjectResult)actionResult.Result!;

                    // Assert - the payload should be InstructorMaterialDto
                    Assert.IsInstanceOfType(okResult.Value, typeof(InstructorMaterialDto), "Expected the OkObjectResult.Value to be InstructorMaterialDto.");

                    var material = (InstructorMaterialDto)okResult.Value!;

                    // Inspect default initializer behavior from the controller's returned DTO
                    Assert.IsNotNull(material);
                    Assert.AreEqual(string.Empty, material.Id, "Expected default Id to be empty string.");
                    Assert.AreEqual(string.Empty, material.Title, "Expected default Title to be empty string.");
                    Assert.AreEqual(string.Empty, material.Type, "Expected default Type to be empty string.");
                    Assert.AreEqual(string.Empty, material.Url, "Expected default Url to be empty string.");
                    Assert.AreEqual(string.Empty, material.Size, "Expected default Size to be empty string.");
                    Assert.AreEqual(string.Empty, material.Date, "Expected default Date to be empty string.");
                    Assert.AreEqual(0, material.Downloads, "Expected default Downloads to be 0.");
                }
            }
        }

        /// <summary>
        /// Verifies that UploadMaterial does not throw for boundary-like courseId inputs and dto fields that are extremely long.
        /// Inputs tested: very long courseId and dto fields (strings > 1000 chars).
        /// Expected: method completes and returns OkObjectResult with an InstructorMaterialDto (default values).
        /// </summary>
        [TestMethod]
        public async Task UploadMaterial_LongInputs_DoesNotThrowAndReturnsOk()
        {
            // Arrange
            var controller = new InstructorController();

            var longCourseId = new string('C', 10_000);
            var longDto = new CreateMaterialDto
            {
                Title = new string('X', 5000),
                Description = new string('D', 5000),
                Type = new string('T', 2000)
            };

            // Act
            Exception? caught = null;
            ActionResult<InstructorMaterialDto>? result = null;
            try
            {
                result = await controller.UploadMaterial(longCourseId, longDto);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert - no exception thrown
            Assert.IsNull(caught, $"Expected no exception, but caught: {caught?.Message}");

            // Assert - valid OkObjectResult with InstructorMaterialDto payload
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result!.Result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result.Result!;
            Assert.IsInstanceOfType(ok.Value, typeof(InstructorMaterialDto));
            var material = (InstructorMaterialDto)ok.Value!;
            Assert.IsNotNull(material);
            Assert.AreEqual(0, material.Downloads);
        }

        // Helper to avoid overly long strings in failure messages
        private static string TruncateForAssertion(string? s, int max = 64)
        {
            if (s is null) return "null";
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }

        /// <summary>
        /// Verifies that UpdateAssignment returns an OkObjectResult containing an InstructorAssignmentDto
        /// for a variety of assignmentId string values and CreateInstructorAssignmentDto.MaxGrade boundary values.
        /// Inputs tested: empty string, whitespace-only, long string, strings with special characters;
        /// MaxGrade values: -1 (invalid/negative), 0, int.MaxValue.
        /// Expected: method completes without throwing and returns OkObjectResult whose Value is an InstructorAssignmentDto.
        /// </summary>
        [TestMethod]
        public async Task UpdateAssignment_VariousAssignmentIdAndDtoValues_ReturnsOkWithInstructorAssignmentDto()
        {
            // Arrange
            var controller = new InstructorController();

            string[] assignmentIds = new[]
            {
                string.Empty,
                "   ",
                new string('a', 1000),
                "special-!@#$%^&*()_+|"
            };

            int[] maxGrades = new[] { -1, 0, int.MaxValue };

            foreach (var assignmentId in assignmentIds)
            {
                foreach (var maxGrade in maxGrades)
                {
                    // Arrange per iteration
                    var dto = new CreateInstructorAssignmentDto
                    {
                        Title = "Sample Title",
                        Description = "Sample Description",
                        CourseCode = "CS101",
                        Deadline = "2099-12-31",
                        MaxGrade = maxGrade,
                        AllowedFormats = new List<string> { "pdf", "docx" }
                    };

                    // Act
                    ActionResult<InstructorAssignmentDto> actionResult = await controller.UpdateAssignment(assignmentId, dto);

                    // Assert - top-level result is OkObjectResult
                    Assert.IsNotNull(actionResult);
                    Assert.IsNotNull(actionResult.Result, "Expected Result to be non-null IActionResult wrapping the response.");
                    Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), $"Expected OkObjectResult for assignmentId='{assignmentId}' and MaxGrade={maxGrade}.");

                    var okResult = actionResult.Result as OkObjectResult;
                    Assert.IsNotNull(okResult?.Value, "OkObjectResult.Value should not be null.");
                    Assert.IsInstanceOfType(okResult.Value, typeof(InstructorAssignmentDto), "OkObjectResult.Value should be an InstructorAssignmentDto.");
                }
            }
        }

        /// <summary>
        /// Verifies that the returned InstructorAssignmentDto from UpdateAssignment contains default property values
        /// (as defined by InstructorAssignmentDto's constructor/initializers) regardless of provided input DTO property values.
        /// Input: a populated CreateInstructorAssignmentDto and a typical assignmentId string.
        /// Expected: returned InstructorAssignmentDto has default/empty values (e.g., empty strings, AllowedFormats empty list, MaxGrade == 0).
        /// </summary>
        [TestMethod]
        public async Task UpdateAssignment_WithPopulatedDto_ReturnedDtoHasDefaultValues()
        {
            // Arrange
            var controller = new InstructorController();

            string assignmentId = "assignment-123";
            var inputDto = new CreateInstructorAssignmentDto
            {
                Title = "Non-default Title",
                Description = "Non-default Description",
                CourseCode = "COURSE-XYZ",
                Deadline = "2025-01-01",
                MaxGrade = 999,
                AllowedFormats = new List<string> { "zip", "exe" }
            };

            // Act
            ActionResult<InstructorAssignmentDto> actionResult = await controller.UpdateAssignment(assignmentId, inputDto);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));

            var okResult = actionResult.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returned = okResult.Value as InstructorAssignmentDto;
            Assert.IsNotNull(returned, "Returned value should be an InstructorAssignmentDto instance.");

            // Verify default-initialized properties from InstructorAssignmentDto
            Assert.AreEqual(string.Empty, returned.Id, "Expected default Id to be empty string.");
            Assert.AreEqual(string.Empty, returned.Title, "Expected default Title to be empty string.");
            Assert.AreEqual(string.Empty, returned.CourseCode, "Expected default CourseCode to be empty string.");
            Assert.AreEqual(string.Empty, returned.CourseName, "Expected default CourseName to be empty string.");
            Assert.AreEqual(string.Empty, returned.Deadline, "Expected default Deadline to be empty string.");
            Assert.AreEqual(0, returned.MaxGrade, "Expected default MaxGrade to be 0.");
            Assert.IsNotNull(returned.AllowedFormats, "AllowedFormats should be non-null (initialized).");
            Assert.AreEqual(0, returned.AllowedFormats.Count, "Expected AllowedFormats to be empty by default.");
            Assert.AreEqual(string.Empty, returned.Status, "Expected default Status to be empty string.");
            Assert.AreEqual(0, returned.SubmissionsCount, "Expected default SubmissionsCount to be 0.");
            Assert.AreEqual(string.Empty, returned.Description, "Expected default Description to be empty string.");
        }
    }
}