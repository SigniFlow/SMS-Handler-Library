namespace SigniFlow.SMSHandler.Exceptions;

[Serializable]
public class InvalidSmsEventTypeException : Exception
{
    public InvalidSmsEventTypeException()
    {
    }

    public InvalidSmsEventTypeException(string message) : base(message)
    {
    }

    public InvalidSmsEventTypeException(string message, Exception inner) : base(message, inner)
    {
    }

    protected InvalidSmsEventTypeException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context)
    {
    }
}