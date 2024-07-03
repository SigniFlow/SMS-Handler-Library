using System.Runtime.InteropServices.JavaScript;

namespace SigniFlow.SMSHandler.Models.HTTP;

public class SmsEvent
{
    public string SmsSecret { get; set; }
    public string Number { get; set; }
    public string EventType { get; set; }
    public string AdditionalData { get; set; }
    public string EventDate { get; set; }
    
    public SmsEvent(string smsSecret, string number, string eventType, string additionalData, string eventDate)
    {
        SmsSecret = smsSecret;
        Number = number;
        EventType = eventType;
        AdditionalData = additionalData;
        EventDate = eventDate;
    }
}