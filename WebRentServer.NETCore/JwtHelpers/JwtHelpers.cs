using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebRentServer.NETCore.JwtHelpers
{
    public static class JwtHelpers
    {
        public static List<Claim> GetClaims(this UserTokens userAccounts, Guid id)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("Id", userAccounts.Id.ToString()),
                new Claim(ClaimTypes.Name, userAccounts.UserName),
                new Claim(ClaimTypes.Email, userAccounts.EmailId),
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddDays(1).ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var userRole in userAccounts.Claims)
            {
                claims.Add(userRole);
            }
            return claims;
        }
        public static List<Claim> GetClaims(this UserTokens userAccounts, out Guid id)
        {
            id = Guid.NewGuid();
            return GetClaims(userAccounts, id);
        }

        public static UserTokens GenTokenKey(UserTokens model, JwtSettings jwtSettings)
        {
            try
            {
                var userToken = new UserTokens();
                if (model == null)
                    throw new ArgumentException(nameof(model));
                var key = Encoding.ASCII.GetBytes(jwtSettings.IssuerSigningKey);
                Guid id = Guid.Empty;
                DateTime expireTime = DateTime.UtcNow.AddDays(1);
                userToken.Validaty = expireTime.TimeOfDay;
                var JWToken = new JwtSecurityToken(
                    issuer: jwtSettings.ValidIssuer,
                    audience: jwtSettings.ValidAudience,
                    claims: GetClaims(model, out id),
                    notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                    expires: new DateTimeOffset(expireTime).DateTime,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));
                userToken.Token = new JwtSecurityTokenHandler().WriteToken(JWToken);
                userToken.UserName = model.UserName;
                userToken.Id = model.Id;
                userToken.GuidId = id;

                return userToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
