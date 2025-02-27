namespace IdentityProvider.Services;

public interface ISmsSender
{
    Task SendSmsAsync(string number, string message);
}

public class SmsSender : ISmsSender
{
    private readonly HttpClient _httpClient;

    public SmsSender(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Consts.SMSeColl);
    }

    public async Task SendSmsAsync(string number, string message)
    {
        var ecallMessage = new EcallMessage
        {
            To = number,
            Content = new EcallContent
            {
                Text = message
            }
        };

        await _httpClient.PostAsJsonAsync("message", ecallMessage);
    }
}
