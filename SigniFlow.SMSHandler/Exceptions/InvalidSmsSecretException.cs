namespace SigniFlow.SMSHandler.Exceptions;

public class InvalidSmsSecretException:Exception
{
    public InvalidSmsSecretException() { }
    public InvalidSmsSecretException(string message) : base(message) { }
    public InvalidSmsSecretException(string message, Exception inner) : base(message, inner) { }
    
    protected InvalidSmsSecretException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}