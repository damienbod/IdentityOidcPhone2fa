using IdentityProvider.Models;
using IdentityProvider.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Pages.Account;

[AllowAnonymous]
public class LoginWith2FaModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginWith2FaModel> _logger;
    private readonly SmsProvider _smsVerifyClient;

    public LoginWith2FaModel(SignInManager<ApplicationUser> signInManager,
        SmsProvider smsVerifyClient, ILogger<LoginWith2FaModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
        _smsVerifyClient = smsVerifyClient;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public bool IsAuthenticator { get; set; }
    public bool IsPhone { get; set; }
    public bool IsEmail { get; set; }

    public class InputModel
    {
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        [DataType(DataType.Text)]
        [Display(Name = "2FA code")]
        public string? TwoFactorCode { get; set; }

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }

        public string Authmethod { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        if (user.AuthenticatorApp2FAEnabled)
        {
            IsAuthenticator = true;
        }
        if (user.Phone2FAEnabled)
        {
            IsPhone = true;
            if (!user.AuthenticatorApp2FAEnabled)
            {
                await _smsVerifyClient.Send2FASmsAsync(user, user.PhoneNumber!);
            }
        }
        if (user.Email2FAEnabled)
        {
            IsEmail = true;
            if (!user.Phone2FAEnabled)
            {
                // Send Email
            }
        }

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        if (Input.TwoFactorCode == null)
        {
            ModelState.AddModelError("Input.TwoFactorCode", "code required");
            UpdateDisplay(user);
            return Page();
        }

        returnUrl ??= Url.Content("~/");

        var code = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        Microsoft.AspNetCore.Identity.SignInResult? result = null;
        if (user.AuthenticatorApp2FAEnabled && (Input.Authmethod != Consts.Phone) && (Input.Authmethod != Consts.Email))
        {
            _logger.LogTrace("User with ID '{UserId}' logged in with 2fa uing Authenticator App.", user.Id);
            result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, rememberMe, Input.RememberMachine);
        }
        else if (user.Phone2FAEnabled && (Input.Authmethod != Consts.Email))
        {
            _logger.LogTrace("User with ID '{UserId}' logged in with 2fa uing Phone (SMS)", user.Id);
            result = await _signInManager.TwoFactorSignInAsync(Consts.Phone, code, rememberMe, Input.RememberMachine);
        }
        else if (user.Email2FAEnabled)
        {
            _logger.LogTrace("User with ID '{UserId}' logged in with 2fa uing Email code", user.Id);
            result = await _signInManager.TwoFactorSignInAsync(Consts.Email, code, rememberMe, Input.RememberMachine);
        }

        if (result!.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
            return LocalRedirect(returnUrl);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return RedirectToPage("./Lockout");
        }
        else
        {
            _logger.LogWarning("Invalid code entered for user with ID '{UserId}'.", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid code.");
            UpdateDisplay(user);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostSendSmsAsync()
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        Input.Authmethod = Consts.Phone;
        await _smsVerifyClient.Send2FASmsAsync(user, user.PhoneNumber!);

        UpdateDisplay(user);

        return Page();
    }

    private void UpdateDisplay(ApplicationUser user)
    {
        if (user.AuthenticatorApp2FAEnabled)
        {
            IsAuthenticator = true;
        }
        if (user.Phone2FAEnabled)
        {
            IsPhone = true;
        }
        if (user.Email2FAEnabled)
        {
            IsEmail = true;
        }
    }

    public async Task<IActionResult> OnPostSendEmailAsync()
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        // await._emailVerifyClient.Send2FAEmailAsync(user, user.Email);

        Input.Authmethod = Consts.Email;
        UpdateDisplay(user);

        return Page();
    }

    public async Task<IActionResult> OnPostUseAuthenticatorAsync()
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        Input.Authmethod = string.Empty;
        UpdateDisplay(user);

        return Page();
    }
}
