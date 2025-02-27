using Duende.IdentityServer.Extensions;
using IdentityProvider.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityProvider.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class ProfilesController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfilesController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("photo")]
    public async Task<string> GetPhotoAsync()
    {
        if (User.IsAuthenticated())
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(user.Photo))
            {
                return user.Photo;
            }
        }

        return string.Empty;
    }
}
