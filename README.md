# SigniFlow SMS Handler Library

A library for APIs that handle SMS events from SigniFlow.

[![Nuget](https://img.shields.io/nuget/v/SigniFlow.SmsHandler) ![Nuget](https://img.shields.io/nuget/dt/SigniFlow.SmsHandler)](https://www.nuget.org/packages/SigniFlow.SmsHandler/)

## Upgrading to 2.0.0

- **Breaking change:** rename the request field `BussinessID` to `BusinessID` in existing form integrations. Both form and JSON endpoints now use `BusinessID`.
- Register `UseSmsEventHandlerJsonApi` to receive JSON requests through the same SMS event handlers.
- Both endpoints accept the optional string field `OtpValue`, exposed as `Event.OtpValue`. Existing requests can omit it.

## Usage

This library
supports [.NET Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-8.0):

```CSharp
using SigniFlow.SMSHandler.API;
using SigniFlow.SMSHandler.Models.Configuration;

// Setup WebApp builder
var builder = WebApplication.CreateBuilder(args);

var secret = builder.Configuration["SmsHandler:Secret"]
    ?? throw new InvalidOperationException("SmsHandler:Secret is required.");
var authOptions = new SmsHandlerAuthOptions(secret); // Same secret as in your SigniFlow business configuration
builder.Services.AddSmsEventHandlerApi<MySmsHandlerImpl>(authOptions); // Implement your own ISmsHandler

// Build WebApp
var app = builder.Build();

// Register API endpoints
app.UseSmsEventHandlerApi("/api/sms/receiver");
app.UseSmsEventHandlerJsonApi("/api/sms/receiver/json");

// Run WebApp
app.Run();
```

Register either endpoint or both. Both use the same scoped `ISmsHandler` implementation and shared-secret configuration.
The original `UseSmsEventHandlerApi` endpoint accepts `application/x-www-form-urlencoded` and `multipart/form-data`.
The additional `UseSmsEventHandlerJsonApi` endpoint accepts `application/json`.

## JSON requests

Send an HTTP `POST` to your configured JSON route with `Content-Type: application/json`:

```json
{
  "SmsSecret": "your-shared-secret",
  "Number": "+27821234567",
  "EventType": "Send OTP",
  "AdditionalData": "optional extra data",
  "eventDate": "2026-10-27T12:00:00",
  "ClientID": "client-123",
  "BusinessID": "business-456",
  "Message": "Your OTP is 001234",
  "OtpValue": "001234"
}
```

All nine fields are strings. JSON field names are case-insensitive under the default ASP.NET Core JSON settings.
Use `SmsSecret` and `BusinessID` in both JSON and form payloads; these map to `Event.Secret` and
`Event.BusinessId` in your handler. `AdditionalData` remains a string, even if it contains serialized JSON.
Both endpoints also accept the optional string field `OtpValue`, available as `Event.OtpValue` in every handler.
Leading zeros are preserved; send JSON values in quotes, for example `"OtpValue": "001234"`.
Omitting `OtpValue`, or sending it as null in JSON, defaults it to an empty string.
Missing or null string fields become empty strings. A missing or invalid `eventDate` defaults to the server's
current local time, matching the form endpoint. Dates are passed to the handler as a formatted string.

| EventType | ISmsHandler method |
| --- | --- |
| `Send OTP` | `HandleSmsOtp()` |
| `Created` | `HandleSmsAccountCreated()` |
| `Authenticate` | `HandleSmsAuthenticate()` |
| `Check Balance` | `HandleSmsGetBalance()` |
| Any other value | `HandleSmsUnkown()` |

Event values are case-insensitive but are not trimmed. All methods read the request from the handler's `Event`
property and return `Task<SmsEventResult>`. Implement each event you need: the default interface implementations
acknowledge success without sending an SMS or checking a balance.

Responses remain plain text: `Suc:EventHandled`, `Err:EventFailed`, or `Suc:<message>` / `Err:<message>` for a custom
`SmsEventResult`. These results use HTTP 200. An incorrect secret throws `InvalidSmsSecretException`, just as it does
for form requests; configure application exception handling if you need a specific HTTP error response.
Malformed, empty, or null JSON bodies return HTTP 400; unsupported content types return HTTP 415 under the default
ASP.NET Core request-binding settings. No handler is called for these invalid requests.
