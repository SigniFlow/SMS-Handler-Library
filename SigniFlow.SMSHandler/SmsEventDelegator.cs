using SigniFlow.SMSHandler.Exceptions;
using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler;

public class SmsEventDelegator
{
    public ISmsHandler SmsHandler { get; set; }
    public SmsEventType SmsEventType { get; set; }

    public SmsEventDelegator(ISmsHandler smsHandler, SmsEventType smsEventType)
    {
        SmsHandler = smsHandler;
        SmsEventType = smsEventType;
    }

    /// <exception cref="InvalidSmsEventTypeException"></exception>
    public async Task<SmsEventResult> HandleSmsEvent()
    {
        return SmsEventType switch
        {
            SmsEventType.SmsOtp => await SmsHandler.HandleSmsOtp(),
            SmsEventType.SmsCreated => await SmsHandler.HandleSmsAccountCreated(),
            SmsEventType.SmsAuthenticate => await SmsHandler.HandleSmsAuthenticate(),
            SmsEventType.Unknown => await SmsHandler.HandleSmsUnkown(),
            _ => throw new InvalidSmsEventTypeException($"Err: Invalid Sms Event Type {SmsEventType}")
        };
    }
}

public enum SmsEventType
{
    SmsOtp,
    SmsCreated,
    SmsAuthenticate,
    Unknown = -1
}