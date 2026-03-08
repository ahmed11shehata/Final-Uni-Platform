using AYA_UIS.Shared.Exceptions;
using Shared.Dtos.ErrorModels;
using Shared.Exceptions;

namespace AYA_UIS.MiddelWares
{
    public class GlobalExceptionHandlingMiddelWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddelWare> _logger;

        public GlobalExceptionHandlingMiddelWare(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddelWare> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                if (context.Response.StatusCode == StatusCodes.Status404NotFound)
                    await HandelExceptionAsync(context );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Somthing Went Wrong {ex.Message}");
                await HandelErrorExceptAsync(context, ex);

            }



        }


        private async Task HandelExceptionAsync(HttpContext content)
        {
            content.Request.ContentType = "application/json";
            var response = new ErrorDetails()
            {
                StatusCode = StatusCodes.Status404NotFound,
                ErrorMessage = $"The end Point {content.Request.Path} Not Found "
            }.ToString();

            await content.Response.WriteAsync(response);
        }

        private async Task HandelErrorExceptAsync(HttpContext content, Exception ex)
        {
            content.Response.ContentType = "application/json";

            //3]Write response in body 
            var response = new ErrorDetails
            {

                ErrorMessage = ex.ToString()

            };

            //1] Change StatusCode 
            content.Response.StatusCode = ex switch
            {
                ValidationException validationException => HandelValidationException(validationException, response),
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                BadRequestException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                PromotionException => StatusCodes.Status400BadRequest,
                InternalServerErrorException => StatusCodes.Status500InternalServerError,
                BaseException baseException => baseException.StatusCode,
                (_) => StatusCodes.Status500InternalServerError
            };

            //2]ChangeContentType 

            response.StatusCode = content.Response.StatusCode;
            await content.Response.WriteAsync(response.ToString());

           

        }

        private int HandelValidationException(ValidationException validationException, ErrorDetails response)
        {
            response.Errors = validationException.Errors;
            return StatusCodes.Status400BadRequest;
        }

    }
}
