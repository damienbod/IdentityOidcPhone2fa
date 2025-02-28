using IdentityProvider.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityProvider.Services;

public class SmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SmsOptions _smsOptions;
    private readonly ILogger<SmsProvider> _logger;

    private const string Message = "message";

    public SmsProvider(IHttpClientFactory clientFactory,
        UserManager<ApplicationUser> userManager,
        IOptions<SmsOptions> smsOptions,
        ILogger<SmsProvider> logger)
    {
        _httpClient = clientFactory.CreateClient(Consts.SMSeColl);
        _userManager = userManager;
        _smsOptions = smsOptions.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> Send2FASmsAsync(ApplicationUser user, string phoneNumber)
    {
        var code = await _userManager.GenerateTwoFactorTokenAsync(user, Consts.Phone);
        var ecallMessage = new EcallMessage
        {
            To = phoneNumber,
            From = _smsOptions.Sender,
            Content = new EcallContent
            {
                Text = $"2FA code: {code}"
            }
        };

        var result = await _httpClient.PostAsJsonAsync(Message, ecallMessage);

        string? messageResult;
        if (result.IsSuccessStatusCode)
        {
            messageResult = await result.Content.ReadAsStringAsync();
        }
        else
        {
            _logger.LogWarning("Error sending SMS 2FA, {ReasonPhrase}", result.ReasonPhrase);
            return (false, result.ReasonPhrase);
        }

        return (true, messageResult);
    }

    public async Task<(bool Success, string? Error)> StartVerificationAsync(ApplicationUser user, string phoneNumber)
    {
        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
        var ecallMessage = new EcallMessage
        {
            To = phoneNumber,
            From = _smsOptions.Sender,
            Content = new EcallContent
            {
                Text = $"Verify code: {token}"
            }
        };

        var result = await _httpClient.PostAsJsonAsync(Message, ecallMessage);

        string? messageResult;
        if (result.IsSuccessStatusCode)
        {
            messageResult = await result.Content.ReadAsStringAsync();
        }
        else
        {
            _logger.LogWarning("Error sending SMS for phone Verification, {ReasonPhrase}", result.ReasonPhrase);
            return (false, result.ReasonPhrase);
        }

        return (true, messageResult);
    }

    public async Task<bool> CheckVerificationAsync(ApplicationUser user, string phoneNumber, string verificationCode)
    {
        var is2faTokenValid = await _userManager
            .VerifyChangePhoneNumberTokenAsync(user, verificationCode, phoneNumber);

        return is2faTokenValid;
    }

    public async Task<(bool Success, string? Error)> EnableSms2FaAsync(ApplicationUser user, string phoneNumber)
    {
        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
        var message = $"Enable phone 2FA code: {token}";

        var ecallMessage = new EcallMessage
        {
            To = phoneNumber,
            From = _smsOptions.Sender,
            Content = new EcallContent
            {
                Text = message
            }
        };

        var result = await _httpClient.PostAsJsonAsync(Message, ecallMessage);

        string? messageResult;
        if (result.IsSuccessStatusCode)
        {
            messageResult = await result.Content.ReadAsStringAsync();
        }
        else
        {
            _logger.LogWarning("Error sending SMS to enable phone 2FA, {ReasonPhrase}", result.ReasonPhrase);
            return (false, result.ReasonPhrase);
        }

        return (true, messageResult);
    }
}
