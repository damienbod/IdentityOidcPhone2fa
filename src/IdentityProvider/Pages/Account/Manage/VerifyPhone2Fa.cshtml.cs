using IdentityProvider.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Pages.Account.Manage;

public class VerifyPhone2FaModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<VerifyPhone2FaModel> _logger;

    public VerifyPhone2FaModel(UserManager<ApplicationUser> userManager, ILogger<VerifyPhone2FaModel> logger)
    {
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
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; } = null!;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogTrace("Unable to load user with ID: {UserId}", _userManager.GetUserId(User));
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyChangePhoneNumberTokenAsync(user, verificationCode, user.PhoneNumber!);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("Input.Code", "Verification code is invalid.");
            return Page();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        user.Phone2FAEnabled = true;
        await _userManager.UpdateAsync(user);

        return RedirectToPage("./TwoFactorAuthentication");
    }
}
