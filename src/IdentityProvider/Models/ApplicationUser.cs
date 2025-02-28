// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Identity;

namespace IdentityProvider.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string Photo { get; set; } = string.Empty;

    public Guid? EntraIdOid { get; set; }

    public Guid? TenantId { get; set; }

    public bool Phone2FAEnabled { get; set; }
    public bool Email2FAEnabled { get; set; }
    public bool AuthenticatorApp2FAEnabled { get; set; }
    public bool Passkeys2FAEnabled { get; set; }
}
