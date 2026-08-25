using System.Net;

namespace Cleanifico.Web.ApiClients;

public sealed class ApiClientException(
    HttpStatusCode statusCode,
    string message,
    string? field = null) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string? Field { get; } = field;
}
