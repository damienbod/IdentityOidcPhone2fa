using IdentityProvider.Models;
using IdentityProvider.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Pages.Account;

[Authorize]
public class VerifyPhoneModel : PageModel
{
    private readonly SmsProvider _smsProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<VerifyPhoneModel> _logger;

    public VerifyPhoneModel(SmsProvider smsProvider, UserManager<ApplicationUser> userManager, ILogger<VerifyPhoneModel> logger)
    {
        _smsProvider = smsProvider;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = null!;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogTrace("Unable to load user with ID: {UserId}", _userManager.GetUserId(User));
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _smsProvider.StartVerificationAsync(user, Input.PhoneNumber);

            if (result.Success)
            {
                return RedirectToPage("ConfirmPhone", new { Input.PhoneNumber });
            }

            _logger.LogTrace("There was an error sending the verification code: {Error}", result.Error);
            ModelState.AddModelError("", $"There was an error sending the verification code: {result.Error}");
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "There was an error sending the verification code, please check the phone number is correct and try again");
        }

        return Page();
    }
}
