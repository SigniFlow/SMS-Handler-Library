using System.Reflection.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SigniFlow.SMSHandler.Models.Configuration;
using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler.API;

public class SmsHandlerAPI
{
    public static async Task<string> HandleSms(ISmsEventHandler ismsEventHandler, Models.HTTP.SmsEvent smsEvent, Models.Configuration.SmsHandlerAuthOptions smsHandlerAuthOptions)
    {
        return (await SmsEventDelegatorFactory.GetSmsEventDelegator(smsEvent, ismsEventHandler, smsHandlerAuthOptions)
            .HandleSmsEvent()).ToString();
    }
    
}

public static class SmsEventHandlerApiExtensions
{
    /// <summary>
    /// Sets up the required DI for the Event Handler
    /// </summary>
    /// <param name="authOptions"><see cref="SmsHandlerAuthOptions"/></param>
    /// <typeparam name="T"> <see cref="ISmsEventHandler"/></typeparam>
    public static IServiceCollection AddSmsEventHandlerApi<T>(this IServiceCollection services, SmsHandlerAuthOptions authOptions) where T : class, ISmsEventHandler
    {
        services.AddScoped<ISmsEventHandler, T>();
        services.AddSingleton<SmsHandlerAuthOptions>(opt=>authOptions);
        return services;
    }
    
    /// <summary>
    /// Creating endpoint for SMS Event Handler
    /// </summary>
    /// <param name="app"></param>
    /// <pram name="route"></param>
    public static void UseSmsEventHandlerApi(this WebApplication app, string route)
    {
        app.MapPost(route,Handler);
    }
    
    private static async Task<string> Handler([FromServices] ISmsEventHandler eventHandler,
        [FromServices] SmsHandlerAuthOptions authOptions, HttpContext ctx)
    {
        if (!ctx.Request.HasFormContentType)
        {
            return "Err: Invalid Content Type";
        }
        
        var form = await ctx.Request.ReadFormAsync();
        var eventDate = DateTime.TryParse(form["eventDate"], out var date) ? date : DateTime.Now;
        var smsEvent = new SmsEvent(form["SmsSecret"],form["Number"],form["EventType"],form["Message"],eventDate.ToString());
        return await SmsHandlerAPI.HandleSms(eventHandler, smsEvent, authOptions);
    }
}