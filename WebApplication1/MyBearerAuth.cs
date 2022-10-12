using System.Security.Claims;
using webapi_authenticator_helper.BearerAuthentication;
using WebApplication1.Controllers;

namespace WebApplication1
{
    public interface IMyBearerAuth
    {
        string Authenticate(string username, string password);
    }

    public class MyBearerAuth : IMyBearerAuth
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration;
        private readonly ITokenHandler _tokenHandler;

        public MyBearerAuth(DatabaseContext databaseContext, IConfiguration configuration, ITokenHandler tokenHandler)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
            _tokenHandler = tokenHandler;
        }

        public string Authenticate(string username, string password)
        {
            User user = _databaseContext.Users.SingleOrDefault(x => x.Username == username && x.Password == password);

            if (user != null)
            {
                string secret = _configuration.GetValue<string>("Token:Secret");

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                return _tokenHandler.GenerateToken(secret: secret, claims: claims, expires: DateTime.Now.AddMinutes(2));
            }
            else
            {
                return null;
            }
        }
    }
}
