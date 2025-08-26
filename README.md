# MFramework.WebApi.Authenticator.Configurator

[![NuGet](https://img.shields.io/nuget/v/MFramework.WebApi.Authenticator.Configurator.svg)](https://www.nuget.org/packages/MFramework.WebApi.Authenticator.Configurator)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MFramework.WebApi.Authenticator.Configurator.svg)](https://www.nuget.org/packages/MFramework.WebApi.Authenticator.Configurator)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-6.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/6.0)
[![GitHub stars](https://img.shields.io/github/stars/muratbaseren/MFramework.WebApi.Authenticator.Configurator.svg)](https://github.com/muratbaseren/MFramework.WebApi.Authenticator.Configurator/stargazers)
[![GitHub issues](https://img.shields.io/github/issues/muratbaseren/MFramework.WebApi.Authenticator.Configurator.svg)](https://github.com/muratbaseren/MFramework.WebApi.Authenticator.Configurator/issues)

.NET Web API Core uygulamaları için kimlik doğrulama ayarlarını kolaylaştıran bir kütüphanedir. Bu paket özellikle Program.cs dosyasında yapılması gereken konfigürasyon kodlarını kolayca yazabilmenizi sağlayan kütüphaneyi içerir.

## 🚀 Özellikler

- ✅ **Basic Authentication** Konfiguratörü & Validatörü
- ✅ **Bearer/JWT Authentication** Konfiguratörü  
- ✅ **SwaggerGen** Konfiguratörü (Basic ve Bearer Auth için)
- ✅ **Token Generator** (JWT için)
- ✅ Kolay kurulum ve kullanım
- ✅ Entity Framework Core entegrasyonu
- ✅ Özelleştirilebilir hata mesajları
- ✅ Claims tabanlı yetkilendirme

## 📦 Kurulum

### NuGet Package Manager
```bash
Install-Package MFramework.WebApi.Authenticator.Configurator
```

### .NET CLI
```bash
dotnet add package MFramework.WebApi.Authenticator.Configurator
```

### PackageReference
```xml
<PackageReference Include="MFramework.WebApi.Authenticator.Configurator" Version="1.0.1" />
```

## 📋 Gereksinimler

- .NET 6.0 veya üzeri
- Microsoft.AspNetCore.App
- Microsoft.AspNetCore.Authentication.JwtBearer (Bearer auth için)
- Swashbuckle.AspNetCore.SwaggerGen (Swagger entegrasyonu için)
- Entity Framework Core (veritabanı işlemleri için)

## 🗄️ Temel Veritabanı EF Code First Kurulumu

**EntityFrameworkCore** NuGet paketlerini eklemeyi unutmayın.
**Add-Migration** ve **Update-Database** komutlarını çalıştırmayı unutmayın.

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

## 🔐 Basic Authentication

### Program.cs
**app.UseAuthentication();** ayarını eklemeyi unutmayın.

```csharp
using Microsoft.AspNetCore.Authentication;
using WebApi.Authenticator.BasicAuthentication;
using webapi_authenticator_helper;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekle
builder.Services.AddControllers();

// Veritabanı bağlantısı
builder.Services.AddDbContext<DatabaseContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Swagger konfigürasyonu
builder.Services.AddSwaggerGen(opts =>
    SwaggerGenBootstrapper.BasicAuthenticationSwaggerSetupAction(opts, "Kullanıcı adı ve şifre bilgilerinizi giriniz."));

// Basic Authentication servisleri
builder.Services.AddScoped<IBasicAuthenticationHandlerOptions<User>, MyBasicAuthOptions>();

builder.Services.AddAuthentication(BasicAuthenticationDefaults.Schema)
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler<User>>(BasicAuthenticationDefaults.Schema, null);

var app = builder.Build();

// HTTP istek pipeline'ı
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // ÖNEMLİ!
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### MyBasicAuthOptions.cs
```csharp
using System.Security.Claims;
using WebApi.Authenticator.BasicAuthentication;

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
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        return claims;
    };

    public string FailMessage => "Kullanıcı adı veya şifre hatalı!";

    private readonly DatabaseContext _databaseContext;

    public MyBasicAuthOptions(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
}
```

### Controller Örneği
**[Authorize]** attribute'unu eklemeyi unutmayın.

```csharp
[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Soğuk", "Serin", "Ilık", "Sıcak", "Çok Sıcak"
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

## 🔑 Bearer Authentication (JWT)

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyAppDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Token": {
    "Secret": "aGVsbG8gd29ybGRfbXlfand0X3NlY3JldF9rZXk="
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Program.cs
**app.UseAuthentication();** ayarını eklemeyi unutmayın.

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using webapi_authenticator_helper;
using webapi_authenticator_helper.BearerAuthentication;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekle
builder.Services.AddControllers();

// Veritabanı bağlantısı
builder.Services.AddDbContext<DatabaseContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Swagger konfigürasyonu
builder.Services.AddSwaggerGen(opts =>
    SwaggerGenBootstrapper.BearerAuthenticationSwaggerSetupAction(opts, "JWT token bilginizi giriniz. (örnek: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...)"));

// Bearer Authentication servisleri
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

var app = builder.Build();

// HTTP istek pipeline'ı
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // ÖNEMLİ!
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### MyBearerAuth.cs
```csharp
using System.Security.Claims;
using webapi_authenticator_helper.BearerAuthentication;

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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            return _tokenHandler.GenerateToken(secret: secret, claims: claims, expires: DateTime.Now.AddMinutes(60));
        }
        else
        {
            return null;
        }
    }
}
```

### Controller Örneği (JWT Authentication Endpoint ile)
**[Authorize]** ve **[AllowAnonymous]** attribute'larını doğru yerlerde kullanmayı unutmayın.

```csharp
[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Soğuk", "Serin", "Ilık", "Sıcak", "Çok Sıcak"
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
    [HttpPost("authenticate")]
    public IActionResult Authenticate(BasicAuthenticationModel model, [FromServices] IMyBearerAuth myBearerAuth)
    {
        string token = myBearerAuth.Authenticate(model.Username, model.Password);

        if (!string.IsNullOrEmpty(token))
        {
            return Ok(new { token = token });
        }
        else
        {
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı!" });
        }
    }

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { userId, username, role });
    }
}
```

## 🛠️ Kullanım Örnekleri

### 1. Basic Authentication ile API Çağrısı
```bash
curl -X GET "https://localhost:5001/WeatherForecast" \
  -H "Authorization: Basic dXNlcm5hbWU6cGFzc3dvcmQ="
```

### 2. JWT Token Alma
```bash
curl -X POST "https://localhost:5001/WeatherForecast/authenticate" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"123456"}'
```

### 3. JWT Token ile API Çağrısı
```bash
curl -X GET "https://localhost:5001/WeatherForecast" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## 🔧 Yapılandırma Seçenekleri

### Token Yapılandırması
```json
{
  "Token": {
    "Secret": "your-secret-key-here",
    "Issuer": "your-app-name",
    "Audience": "your-app-users",
    "ExpirationMinutes": 60
  }
}
```

### Veritabanı Bağlantı Dizesi
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyAppDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

## 📖 API Dokümantasyonu

Swagger UI'da otomatik olarak authentication için gerekli alanlar eklenir:

- **Basic Authentication**: Username ve Password alanları
- **Bearer Authentication**: JWT Token alanı

## 🤝 Katkıda Bulunma

Katkılarınızı memnuniyetle karşılıyoruz! Lütfen şu adımları takip edin:

1. Projeyi fork edin
2. Feature branch'i oluşturun (`git checkout -b feature/AmazingFeature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some AmazingFeature'`)
4. Branch'inizi push edin (`git push origin feature/AmazingFeature`)
5. Pull Request açın

### Geliştirme Ortamı

```bash
# Projeyi klonlayın
git clone https://github.com/muratbaseren/MFramework.WebApi.Authenticator.Configurator.git

# Proje dizinine gidin
cd MFramework.WebApi.Authenticator.Configurator

# Restore ve build
dotnet restore
dotnet build

# Test uygulamasını çalıştırın
cd WebApplication1
dotnet run
```

## 📝 Lisans

Bu proje [Apache License 2.0](LICENSE.txt) lisansı altında lisanslanmıştır.

## 📞 İletişim

- **Proje Sahibi**: Murat Başeren ([@muratbaseren](https://github.com/muratbaseren))
- **Proje URL'si**: [https://github.com/muratbaseren/MFramework.WebApi.Authenticator.Configurator](https://github.com/muratbaseren/MFramework.WebApi.Authenticator.Configurator)
- **NuGet Paketi**: [MFramework.WebApi.Authenticator.Configurator](https://www.nuget.org/packages/MFramework.WebApi.Authenticator.Configurator)

## 💖 Sponsor

Bu projeyi desteklemek için [GitHub Sponsors](https://github.com/sponsors/muratbaseren) sayfasını ziyaret edebilirsiniz.

## 📚 Ek Kaynaklar

- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JWT Bearer Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Swagger/OpenAPI](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
