using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace WebApi.Authenticator.BasicAuthentication
{
    public class BasicAuthenticationHandler<T> : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IBasicAuthenticationHandlerOptions<T> _authenticationHandlerOptions;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IBasicAuthenticationHandlerOptions<T> authenticationHandlerOptions) : base(options, logger, encoder, clock)
        {
            this._authenticationHandlerOptions = authenticationHandlerOptions;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();

            // action anonim e açık ise bypass.
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            if (Request.Headers.ContainsKey("Authorization") == false)
            {
                return AuthenticateResult.Fail("Authorization header does not exist.");
            }

            // Request header da; Authorization "Basic erty1213="
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);  // username:password
            var username = credentials[0];
            var password = credentials[1];

            BasicAuthenticationModel authenticationModel = new()
            {
                Username = username,
                Password = password
            };

            T entity = _authenticationHandlerOptions.AuthenticateExpression(authenticationModel);

            if (entity != null)
            {
                List<Claim> claims = _authenticationHandlerOptions.ClaimsFunction?.Invoke(entity) ?? null;

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);

            }
            else
            {
                return AuthenticateResult.Fail(_authenticationHandlerOptions.FailMessage);
            }

        }
    }
}
