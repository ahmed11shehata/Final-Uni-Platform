using System.IdentityModel.Tokens.Jwt;
using AYA_UIS.Application.Contracts;

namespace AYA_UIS.MiddelWares
{
    /// <summary>
    /// Middleware that checks if the incoming JWT has been blocked (logged out) by jti.
    /// Must be registered AFTER UseAuthentication() and BEFORE UseAuthorization().
    /// </summary>
    public class TokenBlocklistMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenBlocklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var rawToken = authHeader.Substring("Bearer ".Length).Trim();

                if (!string.IsNullOrWhiteSpace(rawToken))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(rawToken);
                        var jti = jwt.Id;

                        if (!string.IsNullOrWhiteSpace(jti))
                        {
                            var blocklist = context.RequestServices.GetRequiredService<ITokenBlocklistService>();
                            if (await blocklist.IsTokenBlockedAsync(jti))
                            {
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync(
                                    "{\"success\":false,\"error\":{\"code\":\"TOKEN_REVOKED\",\"message\":\"Token has been revoked. Please log in again.\"}}");
                                return;
                            }
                        }
                    }
                    catch
                    {
                        // If token can't be parsed, let the auth pipeline handle it
                    }
                }
            }

            await _next(context);
        }
    }
}
