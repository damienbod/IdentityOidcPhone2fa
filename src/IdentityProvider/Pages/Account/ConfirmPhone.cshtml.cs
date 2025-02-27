using IdentityProvider.Models;
using IdentityProvider.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Pages.Account;

[Authorize]
public class ConfirmPhoneModel : PageModel
{
    private readonly SmsVerifyClient _client;
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmPhoneModel(SmsVerifyClient client, UserManager<ApplicationUser> userManager)
    {
        _client = client;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string? VerificationCode { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            if (Input.PhoneNumber != null && Input.VerificationCode != null)
            {
                return await VerifyAndProcessCode(Input.PhoneNumber, Input.VerificationCode);
            }
            else
            {
                ModelState.AddModelError("", "Input.PhoneNumber or Input.VerificationCode missing");
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "There was an error confirming the code, please check the verification code is correct and try again");
        }

        return Page();
    }

    private async Task<IActionResult> VerifyAndProcessCode(string phoneNumber, string code)
    {
        var applicationUser = await _userManager.GetUserAsync(User);

        if (applicationUser != null)
        {
            var validCodeForUserSession = await _client.CheckVerificationAsync(applicationUser,
                phoneNumber, code);

            return await ProcessValidCode(applicationUser, validCodeForUserSession);
        }
        else
        {
            ModelState.AddModelError("", "No user");
            return Page();
        }
    }

    private async Task<IActionResult> ProcessValidCode(ApplicationUser applicationUser, bool validCodeForUserSession)
    {
        if (validCodeForUserSession)
        {
            var phoneNumber = await _userManager.GetPhoneNumberAsync(applicationUser);
            if (Input.PhoneNumber != phoneNumber)
            {
                await _userManager.SetPhoneNumberAsync(applicationUser, Input.PhoneNumber);
            }

            applicationUser.PhoneNumberConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(applicationUser);

            if (updateResult.Succeeded)
            {
                return RedirectToPage("ConfirmPhoneSuccess");
            }
            else
            {
                ModelState.AddModelError("", "There was an error confirming the verification code, please try again");
            }
        }
        else
        {
            ModelState.AddModelError("", "There was an error confirming the verification code");
        }

        return Page();
    }
}
