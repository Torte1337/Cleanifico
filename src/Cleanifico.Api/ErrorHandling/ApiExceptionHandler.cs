using Cleanifico.Application.CleaningObjects;
using Cleanifico.Application.CleaningTypes;
using Cleanifico.Application.Customers;
using Cleanifico.Application.Employees;
using Cleanifico.Application.Security;
using Cleanifico.Application.TimeTypes;
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

            CleaningObjectNotFoundException notFoundException =>
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Objekt nicht gefunden",
                    detail: notFoundException.Message),

            CleaningObjectConflictException conflictException =>
                Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Objekt konnte nicht geändert werden",
                    detail: conflictException.Message,
                    extensions: new Dictionary<string, object?>
                    {
                        ["field"] = conflictException.Field
                    }),

            CustomerNotFoundException notFoundException =>
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kunde nicht gefunden",
                    detail: notFoundException.Message),

            CustomerConflictException conflictException =>
                Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Kunde konnte nicht geändert werden",
                    detail: conflictException.Message,
                    extensions: new Dictionary<string, object?>
                    {
                        ["field"] = conflictException.Field
                    }),

            EmployeeNotFoundException notFoundException =>
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Mitarbeiter nicht gefunden",
                    detail: notFoundException.Message),

            EmployeeConflictException conflictException =>
                Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Mitarbeiter konnte nicht geändert werden",
                    detail: conflictException.Message,
                    extensions: new Dictionary<string, object?>
                    {
                        ["field"] = conflictException.Field
                    }),

            TimeTypeNotFoundException notFoundException =>
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Zeittyp nicht gefunden",
                    detail: notFoundException.Message),

            TimeTypeConflictException conflictException =>
                Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Zeittyp konnte nicht geändert werden",
                    detail: conflictException.Message,
                    extensions: new Dictionary<string, object?>
                    {
                        ["field"] = conflictException.Field
                    }),

            UserValidationException userValidationException =>
                Results.ValidationProblem(
                    userValidationException.Errors,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validierung fehlgeschlagen"),

            UserNotFoundException userNotFoundException =>
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Benutzer nicht gefunden",
                    detail: userNotFoundException.Message),

            UserConflictException userConflictException =>
                Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Benutzer konnte nicht geändert werden",
                    detail: userConflictException.Message,
                    extensions: new Dictionary<string, object?>
                    {
                        ["field"] = userConflictException.Field
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
