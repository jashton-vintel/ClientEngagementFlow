using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ClientEngagementFlow.Api.Tests.Authentication
{
    public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.ContainsKey("X-Test-Anonymous"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var scopes = Request.Headers.TryGetValue("X-Test-Scopes", out var scopeHeader)
                ? scopeHeader.ToString()
                : "jobs.submit";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier,"test-user-id"),
                new(ClaimTypes.Name, "Integration Test User")
            };

            if (!string.IsNullOrWhiteSpace(scopes))
            {
                claims.Add(new Claim("scp", scopes));
            }

            var identity = new ClaimsIdentity(claims,SchemeName);

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal,SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
