using Jarstat.Domain.Entities;
using Jarstat.Domain.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jarstat.Application.Services;

public class JwtAuthenticationService
{
    private string JWT_SECURITY_KEY = string.Empty;
    private int JWT_TOKEN_VALIDITY_MINS = default;

    private readonly UserManager<User> _userManager;

    public JwtAuthenticationService(UserManager<User> userManager)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        JWT_SECURITY_KEY = configuration.GetSection("JwtSecurityKey").Value!;
        JWT_TOKEN_VALIDITY_MINS = Convert.ToInt32(configuration.GetSection("JwtTokenValidityMins").Value!);

        _userManager = userManager;
    }

    public async Task<UserSession?> GenerateJwtToken(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return null;

        var foundUser = await _userManager.FindByNameAsync(userName);
        
        if (foundUser is null)
            return null;

        var passwordCheckingResult = await _userManager.CheckPasswordAsync(foundUser, password);

        if (!passwordCheckingResult)
            return null;

        var tokenExpiryTimeStamp = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDITY_MINS);
        var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);

        var claimsIdentity = new ClaimsIdentity(new List<Claim>
        {
            new Claim("UserId", foundUser.Id.ToString()),
            new Claim(ClaimTypes.Name, foundUser.UserName)
        });

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = tokenExpiryTimeStamp,
            SigningCredentials = signingCredentials
        };

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
        var token = jwtSecurityTokenHandler.WriteToken(securityToken);

        var userSession = new UserSession
        {
            UserId = foundUser.Id,
            UserName = foundUser.UserName,
            Token = token,
            ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds
        };

        return userSession;
    }
}
