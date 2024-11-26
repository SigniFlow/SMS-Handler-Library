using SigniFlow.SMSHandler.Exceptions;
using SigniFlow.SMSHandler.Models.Configuration;
using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler;

public static class SmsEventDelegatorFactory
{
    public static SmsEventDelegator GetSmsEventDelegator(SmsEvent smsEvent, ISmsHandler smsHandler,
        SmsHandlerAuthOptions smsHandlerAuthOptions)
    {
        if (!HasValidAuth(smsEvent, smsHandlerAuthOptions))
        {
            throw new InvalidSmsSecretException("Err: Sms Secret does not match");
        }

        var eventType = ConvertToSMSEventType(smsEvent.EventType);
        smsHandler.Event = smsEvent;
        return new SmsEventDelegator(smsHandler, eventType);
    }

    private static bool HasValidAuth(SmsEvent smsEvent, SmsHandlerAuthOptions smsHandlerAuthOptions)
    {
        return smsEvent.Secret == smsHandlerAuthOptions.Secret;
    }

    public static SmsEventType ConvertToSMSEventType(string smsEvent)
    {
        var typeToTest = smsEvent ?? string.Empty;
        return typeToTest.ToLower() switch
        {
            "send otp" => SmsEventType.SmsOtp,
            "created" => SmsEventType.SmsCreated,
            "authenticate" => SmsEventType.SmsAuthenticate,
            _ => SmsEventType.Unknown
        };
    }
}