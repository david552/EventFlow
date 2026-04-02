using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventFlow.API.infrastructures.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, string key)
        {
            var keybytes = Encoding.ASCII.GetBytes(key);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }


            ).AddJwtBearer(x =>
            x.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(keybytes),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "localhost",
                ValidAudience = "localhost"
            });
            return services;
        }
    }
}
