# ASP.NET Core Identity & OpenID Connect using Phone 2FA

All services use the [SmsProvider.cs](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Services/SmsProvider.cs) to send SMS messages.

This can be updated with any SMS provider.

## Step 1: Verify phone flow

[VerifyPhone](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/VerifyPhone.cshtml.cs)

[ConfirmPhone](https://github.com/damienbod/IdentityOidcPhone2fa/blob/main/src/IdentityProvider/Pages/Account/ConfirmPhone.cshtml.cs)

## Step 2: Enable phone 2FA

[]()

[]()

## Step 3: 2FA phone flow

```csharp
```

## Further flows

### Phone only authentication 

Requires mass usage protection

### Recover account using Phone authentication 

Requires mass usage protection

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