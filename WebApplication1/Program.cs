using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using webapi_authenticator_helper;
using webapi_authenticator_helper.BearerAuthentication;
using WebApplication1.Controllers;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<DatabaseContext>(opts =>
            {
                opts.UseSqlServer("Server=localhost;Database=webapiauthhelperdb;trusted_connection=true");
                opts.UseLazyLoadingProxies();
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();


            //
            //  BASIC AUTHENTICATION
            //
            //builder.Services.AddSwaggerGen(opts =>
            //    SwaggerGenBootstrapper.BasicAuthenticationSwaggerSetupAction(opts, "Buraya kullanýcý adý ve þifre bilgilerinizi giriniz."));

            //builder.Services.AddScoped<IBasicAuthenticationHandlerOptions<User>, MyBasicAuthOptions>();

            //builder.Services.AddAuthentication(BasicAuthenticationDefaults.Schema)
            //    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler<User>>(BasicAuthenticationDefaults.Schema, null);


            //
            //  BEARER AUTHENTICATION
            //
            builder.Services.AddSwaggerGen(opts =>
                SwaggerGenBootstrapper.BearerAuthenticationSwaggerSetupAction(opts, "Buraya token bilginizi giriniz."));

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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}