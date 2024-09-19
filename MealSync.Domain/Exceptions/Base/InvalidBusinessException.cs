using System.Net;

namespace MealSync.Domain.Exceptions.Base;

public class InvalidBusinessException : Exception
{
    public HttpStatusCode HttpStatusCode { get; private set; }

    public object[] Args { get; private set; } = Array.Empty<object>();

    // Default constructor
    public InvalidBusinessException()
        : base()
    {
    }

    // Constructor that accepts a message
    public InvalidBusinessException(string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }

    public InvalidBusinessException(string message, object[] args, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        Args = args;
    }

    // Constructor that accepts a message and an inner exception
    public InvalidBusinessException(string message, Exception innerException, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
    }
}