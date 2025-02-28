using IdentityProvider.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityProvider.Services;

public class SmsVerifyClient
{
    private readonly HttpClient _httpClient;
    private readonly UserManager<ApplicationUser> _userManager;

    public SmsVerifyClient(IHttpClientFactory clientFactory,
        UserManager<ApplicationUser> userManager)
    {
        _httpClient = clientFactory.CreateClient(Consts.SMSeColl);
        _userManager = userManager;
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
