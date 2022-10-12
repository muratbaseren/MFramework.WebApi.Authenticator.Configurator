using System.Security.Claims;

namespace WebApi.Authenticator.BasicAuthentication
{
    public interface IBasicAuthenticationHandlerOptions<T>
    {
        Func<BasicAuthenticationModel, T> AuthenticateExpression { get; }
        Func<T, List<Claim>> ClaimsFunction { get; }
        string FailMessage { get; }
    }

    public class BasicAuthenticationHandlerOptions<T> : IBasicAuthenticationHandlerOptions<T>
    {
        public Func<BasicAuthenticationModel, T> AuthenticateExpression { get; }
        public Func<T, List<Claim>> ClaimsFunction { get; }
        public string FailMessage { get; }

        public BasicAuthenticationHandlerOptions(Func<BasicAuthenticationModel, T> authenticateExpression, Func<T, List<Claim>> claimsFunction, string failMessage = "Username or password is incorrect.")
        {
            AuthenticateExpression = authenticateExpression;
            ClaimsFunction = claimsFunction;
            FailMessage = failMessage;
        }

    }
}
