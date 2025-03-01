# ASP.NET Core Identity & OpenID Connect using Phone 2FA

A demo to implement phone (SMS) verification and two-factor authentication (2FA) using ASP.NET Core Identity. The solution integrates phone-based verification and 2FA mechanisms. The implementation uses ASP.NET Core Identity’s extensibility to incorporate SMS-based verification during user registration and login processes. SMS is no longer a recommended authentication method due to security risks but does provide a good solution for some business cases or user flows like onboarding phone users or phone applications, frontline workers with no desktop or other such solutions.

All services use the [SmsProvider.cs](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Services/SmsProvider.cs) to send SMS messages.

This can be updated with any SMS provider.

## Services setup:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.Configure<SmsOptions>(builder.Configuration.GetSection("SmsOptions"));

var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(
    $"{builder.Configuration["SmsOptions:Username"]}:{builder.Configuration["SmsOptions:Password"]}"));

builder.Services.AddHttpClient(Consts.SMSeColl, client =>
{
    client.BaseAddress = new Uri($"{builder.Configuration["SmsOptions:Url"]}");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
});

builder.Services.AddScoped<SmsProvider>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider)
    .AddTokenProvider<AuthenticatorTokenProvider<ApplicationUser>>(TokenOptions.DefaultAuthenticatorProvider)
    .AddTokenProvider<PhoneNumberTokenProvider<ApplicationUser>>(Consts.Phone)
    .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(Consts.Email);

```

The **ApplicationUser** needs some new properties to support multiple authentication methods. The properties are used to allow a user to use the selected authentication method or force an authentication on a OpenID Connect client.

```csharp
public bool Phone2FAEnabled { get; set; }
public bool Email2FAEnabled { get; set; }
public bool AuthenticatorApp2FAEnabled { get; set; }
public bool Passkeys2FAEnabled { get; set; }
```

## Step 1: Verify phone flow

[VerifyPhone](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/VerifyPhone.cshtml.cs)

[ConfirmPhone](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/ConfirmPhone.cshtml.cs)

## Step 2: Enable phone 2FA flow

[TwoFactorAuthentication](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/Manage/TwoFactorAuthentication.cshtml.cs)

[EnablePhone2Fa](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/Manage/EnablePhone2Fa.cshtml.cs)

[VerifyPhone2Fa](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/Manage/VerifyPhone2Fa.cshtml.cs)

[DisablePhone2Fa](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/Manage/DisablePhone2Fa.cshtml.cs)

> Note
> All Enable and disable Razor Pages require updates to set the enabled authentication method.

## Step 3: 2FA phone flow

[LoginWith2Fa](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/LoginWith2Fa.cshtml.cs)

## Further flows

### Phone only authentication 

Requires mass usage protection, a captcha should be used or an authenticated user.

### Recover account using Phone authentication 

Requires mass usage protection, a captcha should be used or an authenticated user.

## Database

```
Add-Migration "InitialScripts"
```

```
Update-Database
```

## Links

https://learn.microsoft.com/en-us/aspnet/core/security/authentication/2fa

https://ecall-messaging.com/

https://github.com/andrewlock/TwilioSamples/blob/master/src/SendVerificationSmsDemo