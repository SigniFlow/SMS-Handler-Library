using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler;

public interface ISmsHandler
{
    SmsEvent Event { get; set; }

    async Task<SmsEventResult> HandleSmsOtp()
    {
        return await Task.FromResult(SmsEventResult.SuccessfulEvent);
    }

    async Task<SmsEventResult> HandleSmsAccountCreated()
    {
        return await Task.FromResult(SmsEventResult.SuccessfulEvent);
    }

    async Task<SmsEventResult> HandleSmsAuthenticate()
    {
        return await Task.FromResult(SmsEventResult.SuccessfulEvent);
    }

    async Task<SmsEventResult> HandleSmsUnkown()
    {
        return await Task.FromResult(new SmsEventResult(true, "Unknown Event"));
    }
}