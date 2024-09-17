using System.Net;

namespace MealSync.Domain.Exceptions.Base;

public class InvalidBusinessException : Exception
{
    public HttpStatusCode HttpStatusCode { get; private set; }

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

    // Constructor that accepts a message and an inner exception
    public InvalidBusinessException(string message, Exception innerException, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
    }
}