using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace AdminClientUI.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly PhotoService _photoService;
    private readonly IUserTokenManagementService _tokenManagementService;

    public IndexModel(PhotoService photoService, IUserTokenManagementService tokenManagementService)
    {
        _photoService = photoService;
        _tokenManagementService = tokenManagementService;
    }

    [BindProperty]
    public byte[] Photo { get; set; } = [];

    public async Task OnGetAsync()
    {
        // token expires after 1hr, would need to replace this with token managment
        //var accessToken = await HttpContext.GetUserAccessTokenAsync(new UserTokenRequestParameters
        //{
        //    Scope = "shopclientscope"
        //});

        var photo = await _photoService.GetPhotoAsync();
        if (!string.IsNullOrEmpty(photo))
        {
            Photo = Base64UrlEncoder.DecodeBytes(photo);
        }
    }
}
