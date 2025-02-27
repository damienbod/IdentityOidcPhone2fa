namespace IdentityProvider.Services;

public class SmsOptions
{
    public string Url { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Channel { get; set; } = "sms";
}
