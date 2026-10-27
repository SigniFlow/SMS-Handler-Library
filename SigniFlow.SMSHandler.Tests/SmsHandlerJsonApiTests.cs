using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SigniFlow.SMSHandler.API;
using SigniFlow.SMSHandler.Exceptions;
using SigniFlow.SMSHandler.Models.Configuration;
using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler.Tests;

[TestFixture]
public class SmsHandlerJsonApiTests
{
    private const string Secret = "test-shared-secret";
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private HandlerState _state = null!;

    [SetUp]
    public async Task Setup()
    {
        _state = new HandlerState();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Production
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(_state);
        builder.Services.AddSmsEventHandlerApi<RecordingSmsHandler>(new SmsHandlerAuthOptions(Secret));

        _app = builder.Build();
        _app.UseSmsEventHandlerApi("/sms/form");
        _app.UseSmsEventHandlerJsonApi("/sms/json");
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [TestCase("Send OTP", nameof(ISmsHandler.HandleSmsOtp))]
    [TestCase("Created", nameof(ISmsHandler.HandleSmsAccountCreated))]
    [TestCase("Authenticate", nameof(ISmsHandler.HandleSmsAuthenticate))]
    [TestCase("Check Balance", nameof(ISmsHandler.HandleSmsGetBalance))]
    [TestCase("Unknown event", nameof(ISmsHandler.HandleSmsUnkown))]
    [TestCase("sEnD oTp", nameof(ISmsHandler.HandleSmsOtp))]
    public async Task JsonRequest_PassesAllFieldsToTheMatchingHandler(string eventType, string method)
    {
        var fields = CreateFields(eventType);
        using var response = await PostJson(JsonSerializer.Serialize(fields));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Suc:EventHandled"));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/plain"));
        Assert.That(_state.Method, Is.EqualTo(method));
        Assert.That(_state.Event, Is.Not.Null);
        var smsEvent = _state.Event!;
        Assert.Multiple(() =>
        {
            Assert.That(smsEvent.Secret, Is.EqualTo(fields["SmsSecret"]));
            Assert.That(smsEvent.Number, Is.EqualTo(fields["Number"]));
            Assert.That(smsEvent.EventType, Is.EqualTo(eventType));
            Assert.That(smsEvent.AdditionalData, Is.EqualTo(fields["AdditionalData"]));
            Assert.That(smsEvent.EventDate, Is.EqualTo(DateTime.Parse(fields["eventDate"]).ToString()));
            Assert.That(smsEvent.ClientId, Is.EqualTo(fields["ClientID"]));
            Assert.That(smsEvent.BusinessId, Is.EqualTo(fields["BusinessID"]));
            Assert.That(smsEvent.Message, Is.EqualTo(fields["Message"]));
            Assert.That(smsEvent.OtpValue, Is.EqualTo("001234"));
        });
    }

    [Test]
    public async Task JsonRequest_AcceptsCaseInsensitiveFieldNames()
    {
        var fields = CreateFields().ToDictionary(field => field.Key.ToLowerInvariant(), field => field.Value);
        using var response = await PostJson(JsonSerializer.Serialize(fields));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_state.Method, Is.EqualTo(nameof(ISmsHandler.HandleSmsOtp)));
        Assert.That(_state.Event!.BusinessId, Is.EqualTo("business-456"));
        Assert.That(_state.Event.OtpValue, Is.EqualTo("001234"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task JsonRequest_MissingOrNullFieldsUseFormDefaults(bool useNull)
    {
        var fields = new Dictionary<string, string?> { ["SmsSecret"] = Secret };
        if (useNull)
        {
            foreach (var field in CreateFields().Keys.Where(key => key != "SmsSecret"))
                fields[field] = null;
        }

        var before = DateTime.Now.AddSeconds(-1);
        using var response = await PostJson(JsonSerializer.Serialize(fields));
        var after = DateTime.Now;

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_state.Method, Is.EqualTo(nameof(ISmsHandler.HandleSmsUnkown)));
        var smsEvent = _state.Event!;
        Assert.Multiple(() =>
        {
            Assert.That(smsEvent.Number, Is.Empty);
            Assert.That(smsEvent.EventType, Is.Empty);
            Assert.That(smsEvent.AdditionalData, Is.Empty);
            Assert.That(smsEvent.ClientId, Is.Empty);
            Assert.That(smsEvent.BusinessId, Is.Empty);
            Assert.That(smsEvent.Message, Is.Empty);
            Assert.That(smsEvent.OtpValue, Is.Empty);
            Assert.That(DateTime.Parse(smsEvent.EventDate), Is.InRange(before, after));
        });
    }

    [Test]
    public async Task JsonRequest_InvalidDateUsesCurrentLocalTime()
    {
        var fields = CreateFields();
        fields["eventDate"] = "invalid-date";
        var before = DateTime.Now.AddSeconds(-1);
        using var response = await PostJson(JsonSerializer.Serialize(fields));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(DateTime.Parse(_state.Event!.EventDate), Is.InRange(before, DateTime.Now));
    }

    [TestCase(true, "Provider accepted", "Suc:Provider accepted")]
    [TestCase(false, "Provider rejected", "Err:Provider rejected")]
    public async Task JsonRequest_PreservesTheHandlerResult(bool success, string message, string expected)
    {
        _state.Result = new SmsEventResult(success, message);
        using var response = await PostJson(JsonSerializer.Serialize(CreateFields()));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo(expected));
    }

    [TestCase("wrong-secret")]
    [TestCase("TEST-SHARED-SECRET")]
    [TestCase("")]
    [TestCase(null)]
    public void JsonRequest_InvalidSecretDoesNotCallAHandler(string? secret)
    {
        var fields = CreateFields();
        if (secret is null)
            fields.Remove("SmsSecret");
        else
            fields["SmsSecret"] = secret;

        Assert.ThrowsAsync<InvalidSmsSecretException>(async () =>
        {
            using var response = await PostJson(JsonSerializer.Serialize(fields));
        });
        Assert.That(_state.Event, Is.Null);
    }

    [TestCase("{")]
    [TestCase("")]
    [TestCase("null")]
    [TestCase("[]")]
    [TestCase("{\"SmsSecret\":\"test-shared-secret\",\"Number\":123}")]
    [TestCase("{\"SmsSecret\":\"test-shared-secret\",\"OtpValue\":1234}")]
    public async Task JsonRequest_InvalidBodyReturnsBadRequest(string json)
    {
        using var response = await PostJson(json);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(_state.Event, Is.Null);
    }

    [TestCase("text/plain")]
    [TestCase("application/x-www-form-urlencoded")]
    public async Task JsonRequest_UnsupportedContentTypeReturnsUnsupportedMediaType(string contentType)
    {
        using var content = new StringContent(JsonSerializer.Serialize(CreateFields()), Encoding.UTF8, contentType);
        using var response = await _client.PostAsync("/sms/json", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
        Assert.That(_state.Event, Is.Null);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FormRequest_StillWorksAlongsideJson(bool multipart)
    {
        var fields = CreateFields();
        using var content = multipart ? CreateMultipartContent(fields) : new FormUrlEncodedContent(fields);
        using var response = await _client.PostAsync("/sms/form", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Suc:EventHandled"));
        Assert.That(_state.Method, Is.EqualTo(nameof(ISmsHandler.HandleSmsOtp)));
        Assert.That(_state.Event!.BusinessId, Is.EqualTo(fields["BusinessID"]));
        Assert.That(_state.Event.OtpValue, Is.EqualTo("001234"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FormRequest_MissingOtpValueDefaultsToEmptyString(bool multipart)
    {
        var fields = CreateFields();
        fields.Remove("OtpValue");
        using var content = multipart ? CreateMultipartContent(fields) : new FormUrlEncodedContent(fields);
        using var response = await _client.PostAsync("/sms/form", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_state.Event!.OtpValue, Is.Empty);
    }

    [Test]
    public async Task FormRequest_StillRejectsJson()
    {
        using var content = new StringContent(JsonSerializer.Serialize(CreateFields()), Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("/sms/form", content);

        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Err: Invalid Content Type"));
        Assert.That(_state.Event, Is.Null);
    }

    private async Task<HttpResponseMessage> PostJson(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync("/sms/json", content);
    }

    private static Dictionary<string, string> CreateFields(string eventType = "Send OTP") => new()
    {
        ["SmsSecret"] = Secret,
        ["Number"] = "+27821234567",
        ["EventType"] = eventType,
        ["AdditionalData"] = "extra data",
        ["eventDate"] = "2026-10-27T12:00:00",
        ["ClientID"] = "client-123",
        ["BusinessID"] = "business-456",
        ["Message"] = "Your OTP is 001234",
        ["OtpValue"] = "001234"
    };

    private static HttpContent CreateMultipartContent(Dictionary<string, string> fields)
    {
        var content = new MultipartFormDataContent();
        foreach (var field in fields)
            content.Add(new StringContent(field.Value), field.Key);
        return content;
    }

    public class HandlerState
    {
        public SmsEvent? Event { get; set; }
        public string? Method { get; set; }
        public SmsEventResult Result { get; set; } = SmsEventResult.SuccessfulEvent;
    }

    public class RecordingSmsHandler(HandlerState state) : ISmsHandler
    {
        public SmsEvent Event { get; set; } = null!;

        public Task<SmsEventResult> HandleSmsOtp() => Record(nameof(HandleSmsOtp));
        public Task<SmsEventResult> HandleSmsAccountCreated() => Record(nameof(HandleSmsAccountCreated));
        public Task<SmsEventResult> HandleSmsAuthenticate() => Record(nameof(HandleSmsAuthenticate));
        public Task<SmsEventResult> HandleSmsGetBalance() => Record(nameof(HandleSmsGetBalance));
        public Task<SmsEventResult> HandleSmsUnkown() => Record(nameof(HandleSmsUnkown));

        private Task<SmsEventResult> Record(string method)
        {
            state.Event = Event;
            state.Method = method;
            return Task.FromResult(state.Result);
        }
    }
}
