using AuthorizeOnGrpc.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class KerberosAuthenticationOptions : AuthenticationSchemeOptions
{
    public string AdminEmail { get; set; } = "admin@example.com";
    public string AdminPassword { get; set; } = "Admin";
}

public class KerberosAuthenticationHandler : AuthenticationHandler<KerberosAuthenticationOptions>
{
    public KerberosAuthenticationHandler(
        IOptionsMonitor<KerberosAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationString = Request
            .Headers
            .Authorization
            .Where(value => value is not null
                && value.StartsWith(KerberosConstans.KerberosPrefix, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (authorizationString is null)
        {
            Logger.LogDebug("Not found Kerberos authenticate sheme.");
            return AuthenticateResult.NoResult();
        }

        var authorizationValue = authorizationString.Substring(KerberosConstans.KerberosPrefix.Length);

        if (string.IsNullOrEmpty(authorizationValue))
        {
            return AuthenticateResult.Fail("Invalid kerberos credentials format. Empty data.");
        }

        // Валидируем валидатором 
        var identity = GetIdentity(authorizationValue);
        Request.HttpContext.User.AddIdentity(identity);

        if (identity.IsAuthenticated)
        {
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Invalid authentication.");
    }

    private ClaimsIdentity GetIdentity(string kerberosString)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "admin"),
            new Claim(ClaimTypes.Upn, "upn"),
        };

        if (string.Equals(kerberosString, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return new ClaimsIdentity(claims, kerberosString);
        }

        return new ClaimsIdentity(claims);
    }
}