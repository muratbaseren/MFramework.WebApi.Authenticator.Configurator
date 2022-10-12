# MFramework.WebApi.Authenticator.Configurator
It is a library that facilitates the settings that need to be made for .NET Web API Core. This package especially includes the library where you can easily make the configuration codes in the program.cs file.

- Basic Authentication Configurator & Validator
- Bearer Authentication Configurator
- SwaggerGen Configurator for Basic and Bearer Auth
- Token Generator

## Basic Database EF CodeFirst Setup
Don't forget the **EntityFrameworkCore...** nuget packages.
Don't forget the **Add Migration** and **Update-Database** attribute setting.

```csharp
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }


    }

    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(30)]
        public string Username { get; set; }

        [StringLength(30)]
        public string Password { get; set; }

        [StringLength(30)]
        public string Role { get; set; } = "user";
    }
```

## Basic Authentication

### Program.cs
Don't forget the **app.UseAuthentication();** setting.

```csharp
    builder.Services.AddSwaggerGen(opts =>
                    SwaggerGenBootstrapper.BasicAuthenticationSwaggerSetupAction(opts, "Enter your username and password information here."));
    
    builder.Services.AddScoped<IBasicAuthenticationHandlerOptions<User>, MyBasicAuthOptions>();
    
    builder.Services.AddAuthentication(BasicAuthenticationDefaults.Schema)
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler<User>>(BasicAuthenticationDefaults.Schema, null);
```
### MyBasicAuthOptions.cs
```csharp
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

        public string FailMessage => "Username or password is incorrect!";

        private readonly DatabaseContext _databaseContext;

        public MyBasicAuthOptions(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
    }
```
### WeatherForecastController.cs
Don't forget the **[Authorize]** attribute setting.

```csharp
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild"
        };

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
```

## Bearer Authentication(JWT)

### appsettings.json
```json
{
  "Token": {
    "Secret": "aGVsbG8gd29ybGQh"
  },
  ...
  "AllowedHosts": "*"
}
```

### Program.cs
Don't forget the **app.UseAuthentication();** setting.
```csharp
    builder.Services.AddSwaggerGen(opts =>
        SwaggerGenBootstrapper.BearerAuthenticationSwaggerSetupAction(opts, "Enter your jwt token information here.(example; HJkasd6HAS6...)"));

    builder.Services.AddScoped<ITokenHandler, webapi_authenticator_helper.BearerAuthentication.TokenHandler>();
    builder.Services.AddScoped<IMyBearerAuth, MyBearerAuth>();
            
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        string secret = builder.Configuration.GetValue<string>("Token:Secret");
        ITokenHandler tokenHandler = new webapi_authenticator_helper.BearerAuthentication.TokenHandler();
        TokenValidationParameters validationParameters = BearerAuthenticationHandler.CreateTokenValidationParameters();

        BearerAuthenticationHandler.Configure(secret, tokenHandler, validationParameters, opts);
    });
```
### MyBearerAuth.cs
```csharp
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

                return _tokenHandler.GenerateToken(secret: secret, claims: claims, expires: DateTime.Now.AddMinutes(5));
            }
            else
            {
                return null;
            }
        }
    }
```
### WeatherForecastController.cs
Don't forget the **[Authorize]** attribute setting.
Don't forget the **[AllowAnonymous]** attribute setting for **Authenticate** method.

```csharp
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild"
        };

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate(BasicAuthenticationModel model, [FromServices] IMyBearerAuth myBearerAuth)
        {
            string token = myBearerAuth.Authenticate(model.Username, model.Password);

            if (string.IsNullOrEmpty(token) != false)
            {
                return Ok(token);
            }
            else
            {
                return Unauthorized();
            }
        }
    }
```