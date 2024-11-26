namespace SigniFlow.SMSHandler.Models.Configuration;

public class SmsHandlerAuthOptions(string secret)
{
    public string Secret { get; set; } = secret;
}