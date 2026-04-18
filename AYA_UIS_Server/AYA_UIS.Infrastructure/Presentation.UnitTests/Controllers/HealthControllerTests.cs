#nullable enable
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using System;


namespace Presentation.Controllers.UnitTests
{
    /// <summary>
    /// Tests for Presentation.Controllers.HealthController.Get
    /// </summary>
    [TestClass]
    public class HealthControllerTests
    {
        /// <summary>
        /// Verifies that calling Get returns an OkObjectResult containing an anonymous object
        /// with a "status" property equal to "healthy" and a non-null "timestamp" property.
        /// Condition: No input parameters (simple health check).
        /// Expected: IActionResult is OkObjectResult, status == "healthy", timestamp property exists and is a DateTime.
        /// </summary>
        [TestMethod]
        public void Get_WhenCalled_ReturnsOkWithHealthyStatusAndTimestamp()
        {
            // Arrange
            var controller = new Presentation.Controllers.HealthController();

            // Act
            var result = controller.Get();

            // Assert
            // Result should be OkObjectResult
            Assert.IsNotNull(result, "Expected non-null IActionResult.");
            Assert.IsInstanceOfType(result, typeof(OkObjectResult), "Expected OkObjectResult from Get().");

            var okResult = (OkObjectResult)result;
            var value = okResult.Value;
            Assert.IsNotNull(value, "Expected OkObjectResult.Value to be non-null.");

            var valueType = value.GetType();

            // status property
            var statusProp = valueType.GetProperty("status");
            Assert.IsNotNull(statusProp, "Expected anonymous object to have a 'status' property.");
            var statusValue = statusProp.GetValue(value) as string;
            Assert.IsNotNull(statusValue, "Expected 'status' property to be a non-null string.");
            Assert.AreEqual("healthy", statusValue, "Expected status to be 'healthy'.");

            // timestamp property exists and is DateTime
            var tsProp = valueType.GetProperty("timestamp");
            Assert.IsNotNull(tsProp, "Expected anonymous object to have a 'timestamp' property.");
            var tsObj = tsProp.GetValue(value);
            Assert.IsNotNull(tsObj, "Expected 'timestamp' property to be non-null.");
            Assert.IsInstanceOfType(tsObj, typeof(DateTime), "Expected 'timestamp' to be of type DateTime.");
        }

        /// <summary>
        /// Verifies that the returned timestamp is a UTC DateTime and is recent (within an acceptable tolerance).
        /// Condition: No input parameters.
        /// Expected: timestamp.Kind == Utc and timestamp is within +/-5 seconds of the test execution window.
        /// </summary>
        [TestMethod]
        public void Get_Timestamp_IsUtcAndRecent()
        {
            // Arrange
            var controller = new Presentation.Controllers.HealthController();

            // Capture a time window to compare against to avoid flaky timing assertions
            var beforeCall = DateTime.UtcNow;

            // Act
            var result = controller.Get();

            var afterCall = DateTime.UtcNow;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult), "Expected OkObjectResult from Get().");
            var okResult = (OkObjectResult)result;
            var value = okResult.Value;
            Assert.IsNotNull(value, "Expected OkObjectResult.Value to be non-null.");

            var tsProp = value.GetType().GetProperty("timestamp");
            Assert.IsNotNull(tsProp, "Expected anonymous object to have a 'timestamp' property.");
            var tsObj = tsProp.GetValue(value);
            Assert.IsInstanceOfType(tsObj, typeof(DateTime), "Expected 'timestamp' to be of type DateTime.");

            var timestamp = (DateTime)tsObj;

            // Kind should be UTC
            Assert.AreEqual(DateTimeKind.Utc, timestamp.Kind, "Expected timestamp.Kind to be Utc.");

            // timestamp should be between beforeCall.AddSeconds(-5) and afterCall.AddSeconds(5) to account for clock/timing differences
            var lowerBound = beforeCall.AddSeconds(-5);
            var upperBound = afterCall.AddSeconds(5);

            Assert.IsTrue(timestamp >= lowerBound && timestamp <= upperBound,
                $"Expected timestamp to be within [{lowerBound:o}, {upperBound:o}] but was {timestamp:o}.");
        }
    }
}