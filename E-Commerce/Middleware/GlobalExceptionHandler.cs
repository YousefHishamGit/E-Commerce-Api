using System.Net;
using System.Text.Json;
using Application.DTOs.Common;
using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace E_Commerce.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, message, errors) = exception switch
            {
                NotFoundException notFound => (
                    (int)HttpStatusCode.NotFound,
                    notFound.Message,
                    (IEnumerable<string>?)null),

                BadRequestException badRequest => (
                    (int)HttpStatusCode.BadRequest,
                    badRequest.Message,
                    null),

                ValidationException validation => (
                    (int)HttpStatusCode.BadRequest,
                    validation.Message,
                    validation.Errors),

                ConflictException conflict => (
                    (int)HttpStatusCode.Conflict,
                    conflict.Message,
                    null),

                UnauthorizedException unauthorized => (
                    (int)HttpStatusCode.Unauthorized,
                    unauthorized.Message,
                    null),

                ForbiddenException forbidden => (
                    (int)HttpStatusCode.Forbidden,
                    forbidden.Message,
                    null),

                KeyNotFoundException keyNotFound => (
                    (int)HttpStatusCode.NotFound,
                    keyNotFound.Message,
                    null),

                UnauthorizedAccessException => (
                    (int)HttpStatusCode.Unauthorized,
                    "Unauthorized access.",
                    null),

                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    _env.IsDevelopment()
                        ? exception.Message
                        : "An internal server error occurred. Please try again later.",
                    _env.IsDevelopment() && exception.StackTrace != null
                        ? new[] { exception.StackTrace }
                        : null)
            };

            if (statusCode >= 500)
            {
                _logger.LogError(exception, "Unhandled server exception occurred: {Message}", exception.Message);
            }
            else
            {
                _logger.LogWarning("Handled client exception [{StatusCode}]: {Message}", statusCode, exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var response = ApiResponseDto<object?>.Fail(message, statusCode, errors);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await httpContext.Response.WriteAsJsonAsync(response, jsonOptions, cancellationToken);

            return true;
        }
    }
}
