using System.Net;
using System.Text.Json;
using AYA_UIS.Shared.Exceptions;

namespace AYA_UIS.MiddelWares
{
    public class GlobalExceptionHandlingMiddelWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddelWare> _logger;

        public GlobalExceptionHandlingMiddelWare(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddelWare> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            // ── Custom typed exceptions (BaseException hierarchy) ──────────
            catch (BaseException ex)
            {
                _logger.LogWarning(ex, "{ErrorCode}: {Message}", ex.ErrorCode, ex.Message);
                await WriteJsonError(context, (HttpStatusCode)ex.StatusCode,
                    ex.Message, ex.ErrorCode);
            }
            // ── Standard .NET exceptions ───────────────────────────────────
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                await WriteJsonError(context, HttpStatusCode.BadRequest,
                    ex.Message, "VALIDATION_ERROR");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
                await WriteJsonError(context, HttpStatusCode.Unauthorized,
                    "Invalid email or password.", "INVALID_CREDENTIALS");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
                await WriteJsonError(context, HttpStatusCode.NotFound,
                    ex.Message, "NOT_FOUND");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                await WriteJsonError(context, HttpStatusCode.BadRequest,
                    ex.Message, "INVALID_OPERATION");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
                await WriteJsonError(context, HttpStatusCode.InternalServerError,
                    "An unexpected server error occurred. Please try again later.", "INTERNAL_SERVER_ERROR");
            }
        }

        /// <summary>
        /// Writes a standardised JSON error envelope:
        /// { "success": false, "error": { "code": "ENUM", "message": "..." } }
        /// </summary>
        private static Task WriteJsonError(
            HttpContext  context,
            HttpStatusCode statusCode,
            string       message,
            string? code = null)
        {
            context.Response.StatusCode  = (int)statusCode;
            context.Response.ContentType = "application/json";
            var errorResponse = new
            {
                success = false,
                error = new
                {
                    code    = code ?? statusCode.ToString(),
                    message = message
                }
            };
            var body = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(body);
        }
    }
}
