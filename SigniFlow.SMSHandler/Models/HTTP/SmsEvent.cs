using System.Runtime.InteropServices.JavaScript;

namespace SigniFlow.SMSHandler.Models.HTTP;

public class SmsEvent
{
    public string Secret { get; set; }
    public string Number { get; set; }
    public string EventType { get; set; }
    public string AdditionalData { get; set; }
    public string EventDate { get; set; }
    public string ClientId { get; set; }
    public string BusinessId { get; set; }
    public string Message { get; set; }

    public SmsEvent(string secret, string number, string eventType, string additionalData, string eventDate,
        string clientId, string businessId, string message)
    {
        Secret = secret;
        Number = number;
        EventType = eventType;
        AdditionalData = additionalData;
        EventDate = eventDate;
        ClientId = clientId;
        BusinessId = businessId;
        Message = message;
    }
}