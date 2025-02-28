using IdentityProvider.Models;
using IdentityProvider.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityProvider.Pages.Account.Manage;

public class EnablePhone2FaModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SmsVerifyClient _smsVerifyClient;

    public EnablePhone2FaModel(UserManager<ApplicationUser> userManager, SmsVerifyClient smsVerifyClient)
    {
        _userManager = userManager;
        _smsVerifyClient = smsVerifyClient;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Input.PhoneNumber = user.PhoneNumber!;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (user.PhoneNumber != Input.PhoneNumber)
        {
            ModelState.AddModelError("Input.PhoneNumber", "Phone number does not match user user, please update or add phone in your profile");
        }

        await _smsVerifyClient.EnableSms2FaAsync(user, Input.PhoneNumber!);

        return RedirectToPage("./VerifyPhone2Fa", new { Input.PhoneNumber });
    }
}
