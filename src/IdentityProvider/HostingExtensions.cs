using IdentityProvider.Data;
using IdentityProvider.Models;
using IdentityProvider.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

namespace IdentityProvider;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        builder.Services.AddScoped<MsGraphDelegatedService>();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.AddTransient<IEmailSender, EmailSender>();

        builder.Services.Configure<SmsOptions>(builder.Configuration.GetSection("SmsOptions"));
        builder.Services.AddTransient<ISmsSender, SmsSender>();

        var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(
            $"{builder.Configuration["SmsOptions:Username"]}:{builder.Configuration["SmsOptions:Password"]}"));

        builder.Services.AddHttpClient(Consts.SMSeColl, client =>
        {
            client.BaseAddress = new Uri($"{builder.Configuration["SmsOptions:Url"]}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
        });

        builder.Services.AddScoped<SmsVerifyClient>();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider)
            .AddTokenProvider<AuthenticatorTokenProvider<ApplicationUser>>(TokenOptions.DefaultAuthenticatorProvider)
            .AddTokenProvider<PhoneNumberTokenProvider<ApplicationUser>>(Consts.Phone)
            .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(Consts.Email);

        var shopclientUIUrl = builder.Configuration["ShopClientUIUrl"];

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients(shopclientUIUrl!))
            .AddAspNetIdentity<ApplicationUser>();

        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddAuthentication()
            .AddMicrosoftIdentityWebApp(options =>
            {
                builder.Configuration.Bind("AzureAd", options);
                options.SignInScheme = "entraidcookie";
                options.UsePkce = true;
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenResponseReceived = context =>
                    {
                        var idToken = context.TokenEndpointResponse.IdToken;
                        return Task.CompletedTask;
                    }
                };
            }, copt => { }, "EntraID", "entraidcookie", false, "Entra ID")
            .EnableTokenAcquisitionToCallDownstreamApi(["User.Read"])
            .AddMicrosoftGraph()
            .AddDistributedTokenCaches();

        builder.Services.AddAuthentication()
            .AddMicrosoftIdentityWebApp(options =>
            {
                builder.Configuration.Bind("EntraIDAdmin", options);
                options.SignInScheme = "adminentraidcookie";
                options.UsePkce = true;
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenResponseReceived = context =>
                    {
                        var idToken = context.TokenEndpointResponse.IdToken;
                        return Task.CompletedTask;
                    }
                };
            }, copt => { }, "AdminEntraID", "adminentraidcookie", false, "Admin ME-ID")
            .EnableTokenAcquisitionToCallDownstreamApi(["User.Read"])
            .AddMicrosoftGraph()
            .AddDistributedTokenCaches();

        builder.Services.AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
             {
                 var oauthConfig = builder.Configuration.GetSection("ProfileApiConfigurations");
                 options.Authority = oauthConfig["Authority"];
                 options.Audience = oauthConfig["Audience"];
                 options.MapInboundClaims = false;
                 options.TokenValidationParameters.ValidTypes = ["at+jwt"];
             });

        builder.Services.AddControllers();

        builder.Services.AddSecurityHeaderPolicies()
          .SetPolicySelector((PolicySelectorContext ctx) =>
          {
              // Weakened security headers for IDP callback
              if (ctx.HttpContext.Request.Path.StartsWithSegments("/connect"))
              {
                  return SecurityHeadersDefinitionsWeakened.GetHeaderPolicyCollection(
                      builder.Environment.IsDevelopment(),
                      builder.Configuration["AzureAd:Instance"],
                      builder.Configuration["ShopClientUIUrl"]!,
                      builder.Configuration["AdminClientUIUrl"]!);
              }

              return SecurityHeadersDefinitions.GetHeaderPolicyCollection(
                  builder.Environment.IsDevelopment(),
                  builder.Configuration["AzureAd:Instance"],
                  builder.Configuration["ShopClientUIUrl"]!,
                  builder.Configuration["AdminClientUIUrl"]!);
          });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSecurityHeaders();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        app.MapControllers();
        return app;
    }
}