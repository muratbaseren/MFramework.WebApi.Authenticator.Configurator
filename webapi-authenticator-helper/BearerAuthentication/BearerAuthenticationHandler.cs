using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace webapi_authenticator_helper.BearerAuthentication
{
    public class BearerAuthenticationHandler
    {
        /// <summary>
        /// Create TokenValidationParameters with defaults excepts;<br/><br/> 
        /// RequireAudience = false<br/>
        /// RequireExpirationTime = false<br/>
        /// ValidateAudience = false<br/>
        /// ValidateIssuer = false<br/>
        /// ValidateIssuerSigningKey = true
        /// </summary>
        /// <returns></returns>
        public static TokenValidationParameters CreateTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                RequireAudience = false,
                RequireExpirationTime = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true
            };
        }

        public static void Configure(string secret, ITokenHandler tokenHandler, TokenValidationParameters validationParameters, JwtBearerOptions options)
        {
            SymmetricSecurityKey securityKey = tokenHandler.CreateSymmetricSecurityKey(secret);

            options.TokenValidationParameters = validationParameters;
            options.TokenValidationParameters.IssuerSigningKey = securityKey;
        }
    }
}
