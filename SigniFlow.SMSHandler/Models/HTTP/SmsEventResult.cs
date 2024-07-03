namespace SigniFlow.SMSHandler.Models.HTTP;

public class SmsEventResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    
    public SmsEventResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
    
    public static SmsEventResult SuccessfulEvent => new SmsEventResult(true, "EventHandled");
    
    public static SmsEventResult FailedEvent => new SmsEventResult(false, "EventFailed");

    public override string ToString()
    {
        return $"{(Success?"Suc":"Err")}:{Message}";
    }
}