using Cleanifico.Application.CleaningTypes;
using Cleanifico.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace Cleanifico.Api.ErrorHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var result = exception switch
        {
            DomainValidationException validationException =>
                Results.ValidationProblem(
                    validationException.Errors,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validierung fehlgeschlagen"),

            CleaningTypeNotFoundException notFoundException =>
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Reinigungstyp nicht gefunden",
                    detail: notFoundException.Message),

            CleaningTypeConflictException conflictException =>
                Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Reinigungstyp konnte nicht geändert werden",
                    detail: conflictException.Message,
                    extensions: new Dictionary<string, object?>
                    {
                        ["field"] = conflictException.Field
                    }),

            _ => HandleUnexpectedException(exception)
        };

        await result.ExecuteAsync(httpContext);
        return true;
    }

    private IResult HandleUnexpectedException(Exception exception)
    {
        logger.LogError(exception, "An unexpected API error occurred.");

        return Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Interner Serverfehler",
            detail: "Die Anfrage konnte nicht verarbeitet werden.");
    }
}
