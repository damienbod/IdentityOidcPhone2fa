using IdentityProvider.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityProvider.Services;

public class SmsVerifyClient
{
    private readonly HttpClient _httpClient;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SmsOptions _smsOptions;

    public SmsVerifyClient(IHttpClientFactory clientFactory,
        UserManager<ApplicationUser> userManager,
        IOptions<SmsOptions> smsOptions)
    {
        _httpClient = clientFactory.CreateClient(Consts.SMSeColl);
        _userManager = userManager;
        _smsOptions = smsOptions.Value;
    }

    public async Task<(bool Success, string? Error)> Send2FASmsAsync(ApplicationUser user, string phoneNumber)
    {
        var code = await _userManager.GenerateTwoFactorTokenAsync(user, Consts.Phone);
        var ecallMessage = new EcallMessage
        {
            To = phoneNumber,
            Content = new EcallContent
            {
                Text = $"2FA code: {code}"
            }
        };

        var result = await _httpClient.PostAsJsonAsync("message", ecallMessage);

        string? messageResult;
        if (result.IsSuccessStatusCode)
        {
            messageResult = await result.Content.ReadAsStringAsync();
        }
        else
        {
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

        var result = await _httpClient.PostAsJsonAsync("message", ecallMessage);

        string? messageResult;
        if (result.IsSuccessStatusCode)
        {
            messageResult = await result.Content.ReadAsStringAsync();
        }
        else
        {
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
}
