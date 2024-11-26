# SigniFlow SMS Handler Library

A library for APIs that handle SMS events from SigniFlow.

[![Nuget](https://img.shields.io/nuget/v/SigniFlow.SmsHandler) ![Nuget](https://img.shields.io/nuget/dt/SigniFlow.SmsHandler)](https://www.nuget.org/packages/SigniFlow.SmsHandler/)

## Usage

This library
supports [.NET Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-8.0):

```CSharp
// Setup WebApp builder
var builder = WebApplication.CreateBuilder(args);

var myAuthOptions = new SmsHandlerAuthOptions(builder.Configuration["SmsHandler:Secret"]); /// Your secret as set up in your business config in SigniFlow
builder.Services.AddSmsEventHandlerApi<MySmsHandlerImpl>(authOptions); // You'll need to implement your own ISmsHandler

// Build WebApp
var app = builder.Build();

// Register API endpoints
app.UseSmsEventHandlerApi("/api/sms/receiver");

// Run WebApp
app.Run();
```
