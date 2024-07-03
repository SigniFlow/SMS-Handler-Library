using SigniFlow.SMSHandler.Exceptions;
using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler;

public class SmsEventDelegator
{
    public ISmsEventHandler SmsEventHandler { get; set; }
    public SmsEventType SmsEventType { get; set; }
    
    public SmsEventDelegator(ISmsEventHandler smsEventHandler, SmsEventType smsEventType)
    {
        SmsEventHandler = smsEventHandler;
        SmsEventType = smsEventType;
    }
    
    /// <exception cref="InvalidSmsEventTypeException"></exception>
    public async Task<SmsEventResult> HandleSmsEvent()
    {
        return SmsEventType switch
        {
            SmsEventType.SmsOtp => await SmsEventHandler.HandleSmsOtp(),
            SmsEventType.SmsCreated => await SmsEventHandler.HandleSmsAccountCreated(),
            SmsEventType.SmsAuthenticate => await SmsEventHandler.HandleSmsAuthenticate(),
            SmsEventType.Unknown => await SmsEventHandler.HandleSmsUnkown(),
            _=> throw new InvalidSmsEventTypeException($"Err: Invalid Sms Event Type {SmsEventType}")
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