using SigniFlow.SMSHandler.Exceptions;
using SigniFlow.SMSHandler.Models.Configuration;
using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler;

public static class SmsEventDelegatorFactory
{
    public static SmsEventDelegator GetSmsEventDelegator(SmsEvent smsEvent, ISmsEventHandler smsEventHandler, SmsHandlerAuthOptions smsHandlerAuthOptions)
    {
        if (!HasValidAuth(smsEvent, smsHandlerAuthOptions))
        {
            throw new InvalidSmsSecretException("Err: Sms Secret does not match");
        }

        var eventType = ConvertToSMSEventType(smsEvent.EventType);
        smsEventHandler.SMSEvent = smsEvent;
        return new SmsEventDelegator(smsEventHandler, eventType);
    }

    private static bool HasValidAuth(SmsEvent smsEvent, SmsHandlerAuthOptions smsHandlerAuthOptions)
    {
        return smsEvent.SmsSecret == smsHandlerAuthOptions.SmsEventSecret;
    }

    public static SmsEventType ConvertToSMSEventType(string smsEvent)
    {
        var typeToTest = smsEvent?? string.Empty;
        return typeToTest.ToLower() switch
        {
            "send otp" => SmsEventType.SmsOtp,
            "created" => SmsEventType.SmsCreated,
            "authenticate" => SmsEventType.SmsAuthenticate,
            _ => SmsEventType.Unknown
        };
    }
    
}