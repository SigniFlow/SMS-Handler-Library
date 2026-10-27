using System.Text.Json.Serialization;

namespace SigniFlow.SMSHandler.Models.HTTP;

/// <summary>
/// JSON request fields matching the existing SigniFlow SMS form contract.
/// </summary>
public class SmsEventJsonRequest
{
    [JsonPropertyName("SmsSecret")]
    public string Secret { get; set; } = string.Empty;

    [JsonPropertyName("Number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("EventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("AdditionalData")]
    public string AdditionalData { get; set; } = string.Empty;

    [JsonPropertyName("eventDate")]
    public string EventDate { get; set; } = string.Empty;

    [JsonPropertyName("ClientID")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("BusinessID")]
    public string BusinessId { get; set; } = string.Empty;

    [JsonPropertyName("Message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("OtpValue")]
    public string OtpValue { get; set; } = string.Empty;
}
