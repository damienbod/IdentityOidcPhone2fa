using IdentityProvider.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Pages.Account.Manage;

public class VerifyPhone2FaModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyPhone2FaModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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

