using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos;
using Shared.Dtos.AI_Module;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public partial class AIToolsControllerTests
    {
        /// <summary>
        /// Tests that GenerateContent returns an OkObjectResult carrying a GeneratedContentDto
        /// and that the returned GeneratedContentDto.Type matches the request Type for a variety
        /// of type values including expected, unexpected, empty, and whitespace-only types.
        /// Input conditions: several Type strings ("flashcards","summary","quiz","unknown","", "   ").
        /// Expected result: method completes successfully, returns OkObjectResult, and the payload's
        /// Type equals the provided Type and Content is non-null (an object).
        /// </summary>
        [TestMethod]
        public async Task GenerateContent_VariousTypeValues_ReturnsOkAndPreservesType()
        {
            // Arrange
            var controller = new AIToolsController();
            var types = new[]
            {
                "flashcards",
                "summary",
                "quiz",
                "unknownType",
                string.Empty,
                "   "
            };

            foreach (var t in types)
            {
                // Act
                var dto = new GenerateRequestDto
                {
                    Type = t,
                    Content = "sample content",
                    Count = 10
                };

                var actionResult = await controller.GenerateContent(dto).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(actionResult, "ActionResult should not be null.");
                Assert.IsNotNull(actionResult.Result, "ActionResult.Result should not be null for Ok result.");
                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), "Result should be OkObjectResult.");

                var okResult = (OkObjectResult)actionResult.Result!;
                Assert.IsNotNull(okResult.Value, "OkObjectResult.Value should not be null.");
                Assert.IsInstanceOfType(okResult.Value, typeof(GeneratedContentDto), "Value should be GeneratedContentDto.");

                var payload = (GeneratedContentDto)okResult.Value!;
                Assert.AreEqual(t, payload.Type, $"Returned Type should match provided Type '{t}'.");
                Assert.IsNotNull(payload.Content, "Returned Content should be non-null (new object()).");
                // Ensure Content is not simply the original content string (the controller returns new object())
                Assert.AreNotEqual(dto.Content, payload.Content);
            }
        }

        /// <summary>
        /// Tests GenerateContent against various Content string edge cases.
        /// Input conditions: empty string, whitespace-only, very long string, special/control characters, normal text.
        /// Expected result: method returns OkObjectResult with non-null Content (object) and preserves Type.
        /// </summary>
        [TestMethod]
        public async Task GenerateContent_ContentEdgeCases_ReturnsOkWithObjectContent()
        {
            // Arrange
            var controller = new AIToolsController();
            var contents = new[]
            {
                string.Empty,
                " ",
                new string('a', 10000),
                "special:\u0000\u001F\n\t\r",
                "normal text"
            };

            foreach (var content in contents)
            {
                // Act
                var dto = new GenerateRequestDto
                {
                    Type = "summary",
                    Content = content,
                    Count = null
                };

                var actionResult = await controller.GenerateContent(dto).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(actionResult);
                Assert.IsNotNull(actionResult.Result);
                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));

                var okResult = (OkObjectResult)actionResult.Result!;
                Assert.IsNotNull(okResult.Value);
                Assert.IsInstanceOfType(okResult.Value, typeof(GeneratedContentDto));

                var payload = (GeneratedContentDto)okResult.Value!;
                Assert.AreEqual(dto.Type, payload.Type, "Type should be preserved.");
                Assert.IsNotNull(payload.Content, "Generated content should be non-null.");
                Assert.AreEqual(typeof(object), payload.Content!.GetType(), "Content should be a plain object instance created by the controller.");
            }
        }

        /// <summary>
        /// Tests GenerateContent with extreme and boundary Count values.
        /// Input conditions: Count = null, 0, -1, int.MinValue, int.MaxValue.
        /// Expected result: method does not throw and returns OkObjectResult whose Type matches the request Type.
        /// The Count value does not influence the returned Type or Content (controller currently ignores Count).
        /// </summary>
        [TestMethod]
        public async Task GenerateContent_CountBoundaryValues_DoNotAffectResult()
        {
            // Arrange
            var controller = new AIToolsController();
            var counts = new int?[] { null, 0, -1, int.MinValue, int.MaxValue };

            foreach (var c in counts)
            {
                // Act
                var dto = new GenerateRequestDto
                {
                    Type = "flashcards",
                    Content = "edge case content",
                    Count = c
                };

                var actionResult = await controller.GenerateContent(dto).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(actionResult, "ActionResult should not be null.");
                Assert.IsNotNull(actionResult.Result, "Result should not be null.");
                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));

                var okResult = (OkObjectResult)actionResult.Result!;
                Assert.IsNotNull(okResult.Value);
                Assert.IsInstanceOfType(okResult.Value, typeof(GeneratedContentDto));

                var payload = (GeneratedContentDto)okResult.Value!;
                Assert.AreEqual(dto.Type, payload.Type, "Returned Type should equal provided Type regardless of Count.");
                Assert.IsNotNull(payload.Content, "Returned Content should be non-null.");
            }
        }

        /// <summary>
        /// Verifies that Chat returns a single Server-Sent Events (SSE) formatted chunk.
        /// Input conditions:
        /// - A valid ChatRequestDto with a normal message and empty history.
        /// Expected result:
        /// - The method yields exactly one string.
        /// - The yielded string starts with "data: {" and contains the expected greeting content.
        /// - The yielded string ends with two newlines ("\n\n") as SSE chunk terminator.
        /// </summary>
        [TestMethod]
        public async Task Chat_ValidDto_YieldsSseFormattedChunk()
        {
            // Arrange
            var controller = new AIToolsController();
            var dto = new ChatRequestDto
            {
                Message = "Hello",
                History = new List<ChatMessageDto>()
            };

            var results = new List<string>();

            // Act
            await foreach (var chunk in controller.Chat(dto))
            {
                results.Add(chunk);
            }

            // Assert
            Assert.AreEqual(1, results.Count, "Expected exactly one SSE chunk to be yielded.");
            var yielded = results[0];

            StringAssert.StartsWith(yielded, "data: {\"content\": \"", "SSE chunk should start with data JSON prefix.");
            StringAssert.Contains(yielded, "Hello! How can I help you with your studies?", "SSE chunk should include the greeting content.");
            StringAssert.EndsWith(yielded, "\n\n", "SSE chunk should end with two newlines as chunk terminator.");
        }

        /// <summary>
        /// Verifies that Chat's output is stable and independent of dto content.
        /// Input conditions:
        /// - Multiple ChatRequestDto variations including long message and whitespace-only message.
        /// Expected result:
        /// - For every valid dto variant, Chat yields the same static greeting SSE chunk.
        /// - No exceptions are thrown during enumeration.
        /// </summary>
        [TestMethod]
        public async Task Chat_VariousDtos_ReturnsSameStaticGreeting()
        {
            // Arrange
            var controller = new AIToolsController();

            var dtos = new[]
            {
                new ChatRequestDto { Message = string.Empty, History = new List<ChatMessageDto>() },
                new ChatRequestDto { Message = new string('x', 5000), History = new List<ChatMessageDto> { new ChatMessageDto { Role = "user", Content = "previous" } } },
                new ChatRequestDto { Message = "   ", History = new List<ChatMessageDto>() }
            };

            var expected = "data: {\"content\": \"Hello! How can I help you with your studies?\"}" + "\n\n";

            // Act & Assert
            foreach (var dto in dtos)
            {
                var collected = new List<string>();
                await foreach (var chunk in controller.Chat(dto))
                {
                    collected.Add(chunk);
                }

                Assert.AreEqual(1, collected.Count, "Expected exactly one SSE chunk for dto variation.");
                Assert.AreEqual(expected, collected[0], "Chat should return the static greeting irrespective of input dto.");
            }
        }

        /// <summary>
        /// Verifies that ExtractContent returns an OkObjectResult containing an ExtractResponseDto
        /// with empty Content, empty Summary, and a non-null empty Metadata dictionary for a variety
        /// of request DTO Type values (including empty, whitespace, known types, unknown types,
        /// very long strings, and strings with special characters).
        /// Input conditions: multiple ExtractRequestDto instances with different Type values.
        /// Expected result: ActionResult is OkObjectResult (HTTP 200) and ExtractResponseDto properties are default/empty.
        /// </summary>
        [TestMethod]
        public async Task ExtractContent_VariousTypeValues_ReturnsDefaultEmptyResponse()
        {
            // Arrange
            var controller = new AIToolsController();

            var testTypes = new[]
            {
                string.Empty,
                " ",                      // whitespace-only
                "pdf",
                "docx",
                "image",
                "text",
                "unknown-type",
                new string('a', 1024),    // very long string
                "spécial-çhår¶",          // special/unicode characters
                "line1\nline2\t\b"        // control characters
            };

            foreach (var t in testTypes)
            {
                var dto = new ExtractRequestDto
                {
                    Type = t
                };

                // Act
                var actionResult = await controller.ExtractContent(dto).ConfigureAwait(false);

                // Assert - ensure we returned an OkObjectResult
                Assert.IsNotNull(actionResult);
                Assert.IsNotNull(actionResult.Result, "Expected Result to be set on ActionResult");
                Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult), $"Expected OkObjectResult for Type='{t}'");

                var okResult = (OkObjectResult)actionResult.Result!;
                // OkObjectResult.StatusCode may be null but controller.Ok sets 200
                Assert.AreEqual(200, okResult.StatusCode ?? 200, $"Expected HTTP 200 for Type='{t}'");

                // Assert payload
                var payload = okResult.Value as ExtractResponseDto;
                Assert.IsNotNull(payload, $"Expected ExtractResponseDto payload for Type='{t}'");
                Assert.AreEqual(string.Empty, payload!.Content, $"Expected empty Content for Type='{t}'");
                Assert.AreEqual(string.Empty, payload.Summary, $"Expected empty Summary for Type='{t}'");
                Assert.IsNotNull(payload.Metadata, $"Expected non-null Metadata for Type='{t}'");
                Assert.AreEqual(0, payload.Metadata.Count, $"Expected empty Metadata dictionary for Type='{t}'");
            }
        }

        /// <summary>
        /// Verifies that calling ExtractContent with a default-constructed DTO (no modifications)
        /// returns the default empty ExtractResponseDto. This ensures controller does not rely on
        /// specific dto.Type content to produce a non-empty response.
        /// Input conditions: new ExtractRequestDto() with default Type value.
        /// Expected result: OkObjectResult with empty Content, empty Summary, and empty Metadata.
        /// </summary>
        [TestMethod]
        public async Task ExtractContent_DefaultDto_ReturnsDefaultEmptyResponse()
        {
            // Arrange
            var controller = new AIToolsController();
            var dto = new ExtractRequestDto(); // Type defaults to empty string

            // Act
            var actionResult = await controller.ExtractContent(dto).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)actionResult.Result!;
            var payload = okResult.Value as ExtractResponseDto;
            Assert.IsNotNull(payload);
            Assert.AreEqual(string.Empty, payload!.Content);
            Assert.AreEqual(string.Empty, payload.Summary);
            Assert.IsNotNull(payload.Metadata);
            Assert.AreEqual(0, payload.Metadata.Count);
        }
    }
}