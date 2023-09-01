using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Users_app.Context;
using Users_app.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UsersDBContext _context;
    private readonly string _jwtSecretKey; // Replace with a strong secret key

    public AuthController(UsersDBContext context)
    {
        _context = context;
        var keyBytes = new byte[32]; // 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }
        _jwtSecretKey = Convert.ToBase64String(keyBytes);
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLogin userLogin)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userLogin.UserName);

        if (user == null || !VerifyPassword(userLogin.Password, user.PasswordHash))
        {
            // Create a login record with failed login attempt
            await CreateFailedLoginRecord(userLogin.UserName, userLogin.Password);

            return Unauthorized();
        }

        // Create a login record with successful login attempt
        await CreateSuccessfulLoginRecord(userLogin.UserName, userLogin.Password);

        var token = GenerateJwtToken(user);

        return Ok(new { Token = token });
    }

    // Helper methods
    private async Task CreateFailedLoginRecord(string username, string password)
    {
        var loginRecord = new UserLogin
        {
            UserName = username,
            Password = HashPassword(password),
            isLoginSuccessful = false,
            LoginDateTime = DateTime.UtcNow
        };
        _context.UserLogins.Add(loginRecord);
        await _context.SaveChangesAsync();
    }

    private async Task CreateSuccessfulLoginRecord(string username, string password)
    {
        var loginRecord = new UserLogin
        {
            UserName = username,
            Password = password,
            isLoginSuccessful = true,
            LoginDateTime = DateTime.UtcNow
        };
        _context.UserLogins.Add(loginRecord);
        await _context.SaveChangesAsync();
    }

    // Helper method to verify password
    private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
    {
        // Hash the entered password using the same method used during registration
        //string enteredPasswordHash = HashPassword(enteredPassword);

        // Compare the hashed passwords
        return enteredPassword == storedPasswordHash;
    }
    private string HashPassword(string password)
    {
        // Use a strong and secure password hashing algorithm, such as bcrypt
        // You'll need to install a package that supports the chosen hashing algorithm
        // Here's an example using BCrypt.Net library
        // Install-Package BCrypt.Net-Next
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Helper method to generate JWT token
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(24), // Token expiration time
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
