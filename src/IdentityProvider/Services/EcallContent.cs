using System.Text.Json.Serialization;

namespace IdentityProvider.Services;

public class EcallContent
{
    [JsonPropertyName("type")]
    public string ECallType { get; set; } = "Text";
    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;
}
