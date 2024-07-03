namespace SigniFlow.SMSHandler.Models.Configuration;

public class SmsHandlerAuthOptions
{
    public string SmsEventSecret { get; set; }
    
    public SmsHandlerAuthOptions(string smsHandlerSecret)
    {
        SmsEventSecret = smsHandlerSecret;
    }
}