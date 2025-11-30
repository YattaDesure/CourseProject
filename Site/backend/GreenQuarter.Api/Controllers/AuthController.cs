using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GreenQuarter.Api.Models;
using GreenQuarter.Domain.Entities;
using GreenQuarter.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GreenQuarter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // First try to find user in AspNetUsers (new system) - only if table exists
            User? user = null;
            try
            {
                user = await _userManager.FindByEmailAsync(request.Email);
            }
            catch
            {
                // AspNetUsers table doesn't exist yet, skip Identity check
            }
            
            if (user != null && user.IsActive)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (result.Succeeded)
                {
                    var token = GenerateJwtToken(user);
                    var fullName = $"{user.FirstName} {user.LastName}".Trim();

                    return Ok(new LoginResponse
                    {
                        Token = token,
                        Email = user.Email ?? string.Empty,
                        Role = user.Role,
                        FullName = fullName,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty
                    });
                }
            }

            // If not found, try to authenticate against existing Residents table
            ResidentDto? resident = null;
            string role = "User"; // Default role
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                
                using (var command = connection.CreateCommand())
                {
                    // Use parameterized query with proper SQL Server syntax - join with roles
                    command.CommandText = @"
                        SELECT r.ResidentId, r.Email, r.FirstName, r.LastName, r.Patronymic, r.Phone, 
                               COALESCE(rol.RoleName, 'User') as RoleName
                        FROM Residents r
                        LEFT JOIN ResidentRoles rr ON r.ResidentId = rr.ResidentId
                        LEFT JOIN roles rol ON rr.RoleId = rol.RoleId
                        WHERE r.Email = @email AND r.Password = @password";
                    
                    var emailParam = command.CreateParameter();
                    emailParam.ParameterName = "@email";
                    emailParam.Value = request.Email ?? string.Empty;
                    command.Parameters.Add(emailParam);
                    
                    var passwordParam = command.CreateParameter();
                    passwordParam.ParameterName = "@password";
                    passwordParam.Value = request.Password ?? string.Empty;
                    command.Parameters.Add(passwordParam);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            resident = new ResidentDto
                            {
                                ResidentId = reader.GetInt32(0),
                                Email = reader.IsDBNull(1) ? null : (reader.GetValue(1)?.ToString() ?? ""),
                                FirstName = reader.IsDBNull(2) ? string.Empty : (reader.GetValue(2)?.ToString() ?? ""),
                                LastName = reader.IsDBNull(3) ? string.Empty : (reader.GetValue(3)?.ToString() ?? ""),
                                Patronymic = reader.IsDBNull(4) ? null : (reader.GetValue(4)?.ToString() ?? ""),
                                Phone = reader.IsDBNull(5) ? null : (reader.GetValue(5)?.ToString() ?? "")
                            };
                            
                            // Get role name and map to English
                            var roleName = reader.IsDBNull(6) ? "User" : (reader.GetValue(6)?.ToString() ?? "User");
                            if (roleName.Contains("Администратор") || roleName.Contains("Admin"))
                            {
                                role = "Admin";
                            }
                            else if (roleName.Contains("Модератор") || roleName.Contains("Moderator"))
                            {
                                role = "Moderator";
                            }
                            else
                            {
                                role = "User";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue
                Console.WriteLine($"Error querying Residents table: {ex.Message}");
            }

            if (resident != null)
            {
                // Generate token for resident
                var token = GenerateJwtTokenForResident(resident, role);
                var fullName = $"{resident.FirstName} {resident.LastName} {resident.Patronymic}".Trim();

                return Ok(new LoginResponse
                {
                    Token = token,
                    Email = resident.Email ?? string.Empty,
                    Role = role,
                    FullName = fullName,
                    FirstName = resident.FirstName ?? string.Empty,
                    LastName = resident.LastName ?? string.Empty
                });
            }

            return Unauthorized(new { message = "Неверные учетные данные" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Произошла ошибка при входе", error = ex.Message });
        }
    }

    private string GenerateJwtTokenForResident(ResidentDto resident, string role)
    {
        var jwtKey = _configuration["JWT:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var jwtIssuer = _configuration["JWT:Issuer"] ?? "GreenQuarter";
        var jwtAudience = _configuration["JWT:Audience"] ?? "GreenQuarterUsers";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, resident.ResidentId.ToString()),
            new Claim(ClaimTypes.Email, resident.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{resident.FirstName} {resident.LastName}".Trim()),
            new Claim(ClaimTypes.Role, role),
            new Claim("Role", role),
            new Claim("ResidentId", resident.ResidentId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Patronymic = request.Patronymic,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        var token = GenerateJwtToken(user);
        var fullName = $"{user.FirstName} {user.LastName}".Trim();

        return Ok(new LoginResponse
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            FullName = fullName,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty
        });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["JWT:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var jwtIssuer = _configuration["JWT:Issuer"] ?? "GreenQuarter";
        var jwtAudience = _configuration["JWT:Audience"] ?? "GreenQuarterUsers";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new Claim("Role", user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
}

public class ResidentDto
{
    public int ResidentId { get; set; }
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
}

