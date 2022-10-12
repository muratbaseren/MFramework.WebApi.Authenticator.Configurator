using System.Security.Claims;
using WebApi.Authenticator.BasicAuthentication;
using WebApplication1.Controllers;

namespace WebApplication1
{

    public class MyBasicAuthOptions : IBasicAuthenticationHandlerOptions<User>
    {
        public Func<BasicAuthenticationModel, User> AuthenticateExpression => (authModel) =>
        {
            return _databaseContext.Users.SingleOrDefault(
                x => x.Username == authModel.Username && x.Password == authModel.Password);
        };

        public Func<User, List<Claim>> ClaimsFunction => (user) =>
        {
            List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

            return claims;
        };

        public string FailMessage => "Kullanıcı adı ya da şifre hatalı!";

        private readonly DatabaseContext _databaseContext;

        public MyBasicAuthOptions(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
    }
}
