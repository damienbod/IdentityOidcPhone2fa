using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Options;

namespace IdentityProvider.Services;

public interface ISmsSender
{
    Task SendSmsAsync(string number, string message);
}

public class SmsSender : ISmsSender
{
    private readonly HttpClient _httpClient;
    private readonly SmsOptions _smsOptions;

    public SmsSender(IHttpClientFactory clientFactory, IOptions<SmsOptions> smsOptions)
    {
        _httpClient = clientFactory.CreateClient(Consts.SMSeColl);
        _smsOptions = smsOptions.Value;
    }

    public async Task SendSmsAsync(string number, string message)
    {
        var ecallMessage = new EcallMessage
        {
            To = number,
            From = _smsOptions.Sender,
            Content = new EcallContent
            {
                Text = message
            }
        };

        await _httpClient.PostAsJsonAsync("message", ecallMessage);
    }
}
