using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SigniFlow.SMSHandler.Models.Configuration;
using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler.API;

public class SmsHandlerApi
{
    public static async Task<string> HandleSms(ISmsHandler smsHandler, SmsEvent smsEvent,
        SmsHandlerAuthOptions smsHandlerAuthOptions)
    {
        return (await SmsEventDelegatorFactory.GetSmsEventDelegator(smsEvent, smsHandler, smsHandlerAuthOptions)
            .HandleSmsEvent()).ToString();
    }
}

public static class SmsHandlerApiExtensions
{
    /// <summary>
    /// Sets up the required DI for the Event Handler
    /// </summary>
    /// <param name="authOptions"><see cref="SmsHandlerAuthOptions"/></param>
    /// <typeparam name="T"> <see cref="ISmsHandler"/></typeparam>
    public static IServiceCollection AddSmsEventHandlerApi<T>(this IServiceCollection services,
        SmsHandlerAuthOptions authOptions) where T : class, ISmsHandler
    {
        services.AddScoped<ISmsHandler, T>();
        services.AddSingleton<SmsHandlerAuthOptions>(opt => authOptions);
        return services;
    }

    /// <summary>
    /// Creating endpoint for SMS Event Handler
    /// </summary>
    /// <param name="app"></param>
    /// <pram name="route"></param>
    public static void UseSmsEventHandlerApi(this WebApplication app, string route)
    {
        app.MapPost(route, Handler);
    }

    /// <summary>
    /// Creates a POST endpoint that accepts SMS events as JSON and uses the same handlers as the form endpoint.
    /// </summary>
    /// <param name="app">The application hosting the endpoint.</param>
    /// <param name="route">The route at which to receive JSON SMS events.</param>
    public static void UseSmsEventHandlerJsonApi(this WebApplication app, string route)
    {
        app.MapPost(route, JsonHandler);
    }

    private static async Task<string> JsonHandler([FromServices] ISmsHandler smsHandler,
        [FromServices] SmsHandlerAuthOptions authOptions, [FromBody] SmsEventJsonRequest request)
    {
        var eventDate = DateTime.TryParse(request.EventDate, out var date) ? date : DateTime.Now;
        var smsEvent = new SmsEvent(request.Secret ?? string.Empty, request.Number ?? string.Empty,
            request.EventType ?? string.Empty, request.AdditionalData ?? string.Empty, eventDate.ToString(),
            request.ClientId ?? string.Empty, request.BusinessId ?? string.Empty, request.Message ?? string.Empty)
        {
            OtpValue = request.OtpValue ?? string.Empty
        };
        return await SmsHandlerApi.HandleSms(smsHandler, smsEvent, authOptions);
    }

    private static async Task<string> Handler([FromServices] ISmsHandler smsHandler,
        [FromServices] SmsHandlerAuthOptions authOptions, HttpContext ctx)
    {
        if (!ctx.Request.HasFormContentType)
        {
            return "Err: Invalid Content Type";
        }

        var form = await ctx.Request.ReadFormAsync();
        var eventDate = DateTime.TryParse(form["eventDate"], out var date) ? date : DateTime.Now;
        var smsEvent = new SmsEvent(form["SmsSecret"], form["Number"], form["EventType"], form["AdditionalData"],
            eventDate.ToString(), form["ClientID"], form["BusinessID"], form["Message"])
        {
            OtpValue = form["OtpValue"].ToString()
        };
        return await SmsHandlerApi.HandleSms(smsHandler, smsEvent, authOptions);
    }
}
