namespace MealSync.Domain.Exceptions.Base;

public class InvalidBusinessException : Exception
{
    // Default constructor
    public InvalidBusinessException()
        : base()
    {
    }

    // Constructor that accepts a message
    public InvalidBusinessException(string message)
        : base(message)
    {
    }

    // Constructor that accepts a message and an inner exception
    public InvalidBusinessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}