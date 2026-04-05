namespace TMS.Application.Exceptions;

/// <summary>
/// Valhalla could not be reached, timed out, or returned a server error. Maps to HTTP 503 for API clients.
/// </summary>
public sealed class ValhallaUnavailableException : Exception
{
    public ValhallaUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
