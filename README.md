# SigniFlow-SmsHandler-Library
A library for creating SMS Handlers

[![Nuget](https://img.shields.io/nuget/v/SigniFlow.SMSHandler) ![Nuget](https://img.shields.io/nuget/dt/SigniFlow.SMSHandler)](https://www.nuget.org/packages/SigniFlow.SMSHandler/)

To learn what an Event Handler is, please consult our [Event Handler product page](https://www.signiflow.com/connect-with-eventhandler/)

## Usage

This library supports [.NET 6 Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0):

```CSharp
// Setup WebApp builder
var builder = WebApplication.CreateBuilder(args);

var myAuthOptions = new SmsHandlerAuthOptions(builder.Configuration["SmsHandler:Secret"]); /// Your secret as set up in your business config in SigniFlow
builder.Services.AddSmsEventHandlerApi<QIIBSmsHandlerImpl>(authOptions); // You'll need to implement your own IEventHandler

// Build WebApp
var app = builder.Build();

// Register event handler API
app.UseSmsEventHandlerApi("/api/SmsHandler/Receiver");

// Run WebApp
app.Run();
```
