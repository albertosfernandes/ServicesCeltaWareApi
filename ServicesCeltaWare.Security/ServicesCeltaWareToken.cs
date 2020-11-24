using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.Security
{
    public class ServicesCeltaWareToken
    {
        public static async Task<string> GetToken()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "servicesCeltaware"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var servicesCeltaWareKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("keyservicesCeltaWare"));
            var credentials = new SigningCredentials(servicesCeltaWareKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "ServicesCeltaWare",
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
