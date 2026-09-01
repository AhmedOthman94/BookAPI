using BookAPI.Entity;
using BookAPI.Exceptions;
using BookAPI.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace BookAPI.Middleware;

public class GlobalExceptionHandler(
	ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		var traceId = httpContext.TraceIdentifier;

		logger.LogError(
			exception,
			"An error occurred while processing request {Path} [TraceId: {TraceId}]",
			httpContext.Request.Path,
			traceId);

		var (statusCode, message, errors) = exception switch
		{
			ValidationException validationEx => (
				StatusCodes.Status400BadRequest,
				"One or more validation errors occurred.",
				validationEx.Errors.Select(e => e.ErrorMessage).ToList()
			),

			DomainException domainEx => (
				domainEx.StatusCode,
				domainEx.Message,
				new List<string> { domainEx.Message }
			),

			ArgumentException argEx => (
				StatusCodes.Status400BadRequest,
				argEx.Message,
				new List<string> { argEx.Message }
			),

			UnauthorizedAccessException => (
				StatusCodes.Status401Unauthorized,
				"You are not authorized to access this resource.",
				new List<string> { "Unauthorized access." }
			),

			_ => (
				StatusCodes.Status500InternalServerError,
				"An unexpected server error occurred. Please try again later.",
				new List<string> { $"An internal error occurred. Ref TraceID: {traceId}" }
			)
		};

		var response = new ApiResponse<object>
		{
			Success = false,
			Message = message,
			Errors = errors
		};

		httpContext.Response.StatusCode = statusCode;

		await httpContext.Response.WriteAsJsonAsync(
			response,
			cancellationToken);

		return true;
	}
}