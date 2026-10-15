using System.Net;

namespace AsistenteIA.Services;

public enum AiErrorKind
{
    Validation,
    MissingKey,
    NoConnection,
    Unauthorized,
    PaymentRequired,
    RateLimited,
    Timeout,
    ServerError,
    BadResponse
}

// Error de dominio con un mensaje comprensible para el usuario.
public class AiServiceException : Exception
{
    public AiErrorKind Kind { get; }
    public HttpStatusCode? StatusCode { get; }

    public AiServiceException(AiErrorKind kind, string message, HttpStatusCode? statusCode = null, Exception? inner = null)
        : base(message, inner)
    {
        Kind = kind;
        StatusCode = statusCode;
    }
}
