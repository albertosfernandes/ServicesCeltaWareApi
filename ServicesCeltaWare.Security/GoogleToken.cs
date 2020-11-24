using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.Security
{
    public class GoogleToken
    {
        public static async Task<string> Get()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Iss, "teste-868@servicesceltainfra.iam.gserviceaccount.com"), //The email address of the service account.
                //new Claim(ClaimTypes.Name, "", "scope"), 
                new Claim(type:"scope", value: "https://www.googleapis.com/auth/drive"), //A space-delimited list of the permissions that the application requests.
                new Claim(JwtRegisteredClaimNames.Aud, "https://oauth2.googleapis.com/token."), //When making an access token request this value is always
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.Now.AddMinutes(50).ToString()), // The expiration time of the assertion, specified as seconds since 00:00:00 UTC, January 1, 1970. This value has a maximum of 1 hour after the issued time.
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.AddMinutes(50).ToString())
            };

            var servicesCeltaWareKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("keyservicesCeltaWare"));
            var credentials = new SigningCredentials(servicesCeltaWareKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                // issuer: "ServicesCeltaWare",
                audience: "clientGoogle",
                claims: claims,
                signingCredentials: credentials,
                expires: DateTime.Now.AddMinutes(30)
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }
    }
}
