using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace webapi_authenticator_helper.BearerAuthentication
{
    public interface ITokenHandler
    {
        SymmetricSecurityKey CreateSymmetricSecurityKey(string secret);
        string GenerateToken(string secret, string issuer = null, string audience = null, DateTime? expires = null, List<Claim> claims = null);
    }

    public class TokenHandler : ITokenHandler
    {
        public SymmetricSecurityKey CreateSymmetricSecurityKey(string secret)
        {
            byte[] key = Encoding.UTF8.GetBytes(secret);
            return new SymmetricSecurityKey(key);
        }

        public string GenerateToken(string secret, string issuer = null, string audience = null, DateTime? expires = null, List<Claim> claims = null)
        {
            SigningCredentials credentials = new SigningCredentials(CreateSymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);

            JwtSecurityToken jwtSecurityToken =
                new JwtSecurityToken(issuer: issuer, audience: audience,
                    claims: claims, expires: expires, signingCredentials: credentials);

            string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return token;
        }
    }
}
