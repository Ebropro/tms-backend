using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TmsApi.Api.Auth;


public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TimeProvider timeProvider)
        : base(options, logger, encoder)
    {
        // TimeProvider is now preferred in modern .NET
    }

    /// Core authentication logic.
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        //simulate anonymous user (force 401 path)
        return Task.FromResult(AuthenticateResult.Fail("No credentials"));
    }
}