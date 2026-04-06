using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GeorgeStore.Extensions;

public static class WebApplicationBuilderExtension
{
    public static WebApplicationBuilder AddJWT(this WebApplicationBuilder builder)
    {
        var jwtOptions = builder.Configuration.GetSection("JWT").Get<JwtOptions>();
        if (jwtOptions is null)
            throw new Exception("Can't be null Jwt options");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Key)
                ),

                ClockSkew = TimeSpan.Zero
            };
        });

        return builder;
    }
}

public record JwtOptions( string Issuer, string Key, string Audience);
