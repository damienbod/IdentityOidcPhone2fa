using AdminClientUI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Scope.Add("profile");
    options.Scope.Add("shopclientscope");
    options.Scope.Add("offline_access");

    var oidcConfig = builder.Configuration.GetSection("OpenIDConnectSettings");
    options.Authority = oidcConfig["Authority"];
    options.ClientId = oidcConfig["ClientId"];
    options.ClientSecret = oidcConfig["ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            return Task.FromResult(0);
        }
    };
});

builder.Services.AddOpenIdConnectAccessTokenManagement();

var profileApiBaseUrl = builder.Configuration["AuthConfigurations:ProtectedApiUrl"];
// registers HTTP client that uses the managed client access token
builder.Services.AddUserAccessTokenHttpClient("profileClient",
    configureClient: client => { client.BaseAddress = new Uri(profileApiBaseUrl!); });

var requireAuthPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(requireAuthPolicy);

builder.Services.AddScoped<PhotoService>();
builder.Services.AddHttpClient();
builder.Services.Configure<AuthConfigurations>(
    builder.Configuration.GetSection("AuthConfigurations"));

builder.Services.AddRazorPages();

builder.Services.AddSecurityHeaderPolicies()
    .SetPolicySelector((PolicySelectorContext ctx) =>
    {
        return SecurityHeadersDefinitions.GetHeaderPolicyCollection(
            builder.Environment.IsDevelopment(),
            builder.Configuration["OpenIDConnectSettings:Authority"]);
    });

var app = builder.Build();

//IdentityModelEventSource.ShowPII = true;
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseSecurityHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
