using IdentityProvider.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityProvider.Pages.Account.Manage;

public class DisablePhone2FaModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DisablePhone2FaModel> _logger;

    public DisablePhone2FaModel(
        UserManager<ApplicationUser> userManager,
        ILogger<DisablePhone2FaModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogTrace("Unable to load user with ID: {UserId}", _userManager.GetUserId(User));
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            throw new InvalidOperationException($"Cannot disable 2FA for user with ID '{_userManager.GetUserId(User)}' as it's not currently enabled.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogTrace("Unable to load user with ID: {UserId}", _userManager.GetUserId(User));
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        user.Phone2FAEnabled = false;

        await _userManager.UpdateAsync(user);

        if (!user.Passkeys2FAEnabled && !user.AuthenticatorApp2FAEnabled && !user.Email2FAEnabled)
        {
            var disable2FaResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2FaResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{_userManager.GetUserId(User)}'.");
            }
        }

        _logger.LogInformation("User with ID '{UserId}' has disabled phone 2fa.", _userManager.GetUserId(User));
        StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup a second factor";
        return RedirectToPage("./TwoFactorAuthentication");
    }
}
