using System.Data.Common;
using GreenQuarter.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenQuarter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] string? role)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var users = new List<object>();

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
                    r.ResidentId as Id,
                    r.Email,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Phone,
                    COALESCE(rol.RoleName, 'User') as RoleName
                FROM Residents r
                LEFT JOIN ResidentRoles rr ON r.ResidentId = rr.ResidentId
                LEFT JOIN roles rol ON rr.RoleId = rol.RoleId
                WHERE 1=1";

            if (!string.IsNullOrEmpty(search))
            {
                sql += " AND (r.FirstName LIKE @search OR r.LastName LIKE @search OR r.Email LIKE @search)";
                var param = command.CreateParameter();
                param.ParameterName = "@search";
                param.Value = $"%{search}%";
                command.Parameters.Add(param);
            }

            command.CommandText = sql;

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var roleName = reader.IsDBNull(6) ? "User" : (reader.GetValue(6)?.ToString() ?? "User");
                    string mappedRole = "User";
                    if (roleName.Contains("Администратор") || roleName.Contains("Admin"))
                    {
                        mappedRole = "Admin";
                    }
                    else if (roleName.Contains("Модератор") || roleName.Contains("Moderator"))
                    {
                        mappedRole = "Moderator";
                    }

                    if (!string.IsNullOrEmpty(role) && mappedRole != role)
                    {
                        continue;
                    }

                    users.Add(new
                    {
                        Id = reader.GetInt32(0).ToString(),
                        Email = reader.IsDBNull(1) ? "" : (reader.GetValue(1)?.ToString() ?? ""),
                        FirstName = reader.IsDBNull(2) ? "" : (reader.GetValue(2)?.ToString() ?? ""),
                        LastName = reader.IsDBNull(3) ? "" : (reader.GetValue(3)?.ToString() ?? ""),
                        Patronymic = reader.IsDBNull(4) ? "" : (reader.GetValue(4)?.ToString() ?? ""),
                        Phone = reader.IsDBNull(5) ? "" : (reader.GetValue(5)?.ToString() ?? ""),
                        Role = mappedRole,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        if (!int.TryParse(id, out var residentId))
        {
            return BadRequest();
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT 
                    r.ResidentId,
                    r.Email,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Phone,
                    COALESCE(rol.RoleName, 'User') as RoleName
                FROM Residents r
                LEFT JOIN ResidentRoles rr ON r.ResidentId = rr.ResidentId
                LEFT JOIN roles rol ON rr.RoleId = rol.RoleId
                WHERE r.ResidentId = @id";

            var param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = residentId;
            command.Parameters.Add(param);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var roleName = reader.IsDBNull(6) ? "User" : (reader.GetValue(6)?.ToString() ?? "User");
                    string mappedRole = "User";
                    if (roleName.Contains("Администратор") || roleName.Contains("Admin"))
                    {
                        mappedRole = "Admin";
                    }
                    else if (roleName.Contains("Модератор") || roleName.Contains("Moderator"))
                    {
                        mappedRole = "Moderator";
                    }

                    var user = new
                    {
                        Id = reader.GetInt32(0).ToString(),
                        Email = reader.IsDBNull(1) ? "" : (reader.GetValue(1)?.ToString() ?? ""),
                        FirstName = reader.IsDBNull(2) ? "" : (reader.GetValue(2)?.ToString() ?? ""),
                        LastName = reader.IsDBNull(3) ? "" : (reader.GetValue(3)?.ToString() ?? ""),
                        Patronymic = reader.IsDBNull(4) ? "" : (reader.GetValue(4)?.ToString() ?? ""),
                        Phone = reader.IsDBNull(5) ? "" : (reader.GetValue(5)?.ToString() ?? ""),
                        Role = mappedRole,
                        IsActive = true
                    };
                    return Ok(user);
                }
            }
        }

        return NotFound();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        if (!int.TryParse(id, out var residentId))
        {
            return BadRequest();
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Residents 
                SET FirstName = @firstName,
                    LastName = @lastName,
                    Patronymic = @patronymic,
                    Phone = @phone,
                    Email = @email
                WHERE ResidentId = @id";

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = residentId;
            command.Parameters.Add(idParam);

            var firstNameParam = command.CreateParameter();
            firstNameParam.ParameterName = "@firstName";
            firstNameParam.Value = request.FirstName ?? "";
            command.Parameters.Add(firstNameParam);

            var lastNameParam = command.CreateParameter();
            lastNameParam.ParameterName = "@lastName";
            lastNameParam.Value = request.LastName ?? "";
            command.Parameters.Add(lastNameParam);

            var patronymicParam = command.CreateParameter();
            patronymicParam.ParameterName = "@patronymic";
            patronymicParam.Value = request.Patronymic ?? (object)DBNull.Value;
            command.Parameters.Add(patronymicParam);

            var phoneParam = command.CreateParameter();
            phoneParam.ParameterName = "@phone";
            phoneParam.Value = request.Phone ?? (object)DBNull.Value;
            command.Parameters.Add(phoneParam);

            var emailParam = command.CreateParameter();
            emailParam.ParameterName = "@email";
            emailParam.Value = request.Email ?? "";
            command.Parameters.Add(emailParam);

            await command.ExecuteNonQueryAsync();
        }

        return NoContent();
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleRequest request)
    {
        if (!int.TryParse(id, out var residentId))
        {
            return BadRequest();
        }

        // Map English role to Russian role ID
        int roleId = 1; // Default to User
        if (request.Role == "Admin")
        {
            roleId = 3; // Администратор
        }
        else if (request.Role == "Moderator")
        {
            roleId = 2; // Модератор
        }
        else
        {
            roleId = 1; // User
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using (var command = connection.CreateCommand())
        {
            // Delete existing role
            command.CommandText = "DELETE FROM ResidentRoles WHERE ResidentId = @id";
            var deleteParam = command.CreateParameter();
            deleteParam.ParameterName = "@id";
            deleteParam.Value = residentId;
            command.Parameters.Add(deleteParam);
            await command.ExecuteNonQueryAsync();

            // Insert new role
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO ResidentRoles (ResidentId, RoleId) VALUES (@id, @roleId)";
            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = residentId;
            command.Parameters.Add(idParam);

            var roleIdParam = command.CreateParameter();
            roleIdParam.ParameterName = "@roleId";
            roleIdParam.Value = roleId;
            command.Parameters.Add(roleIdParam);

            await command.ExecuteNonQueryAsync();
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        // Check if email already exists
        using (var checkCommand = connection.CreateCommand())
        {
            checkCommand.CommandText = "SELECT COUNT(*) FROM Residents WHERE Email = @email";
            var emailCheckParam = checkCommand.CreateParameter();
            emailCheckParam.ParameterName = "@email";
            emailCheckParam.Value = request.Email ?? "";
            checkCommand.Parameters.Add(emailCheckParam);

            var count = (int)(await checkCommand.ExecuteScalarAsync() ?? 0);
            if (count > 0)
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }
        }

        // Insert new resident
        int newResidentId;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                INSERT INTO Residents (Email, Password, FirstName, LastName, Patronymic, Phone)
                OUTPUT INSERTED.ResidentId
                VALUES (@email, @password, @firstName, @lastName, @patronymic, @phone)";

            var emailParam = command.CreateParameter();
            emailParam.ParameterName = "@email";
            emailParam.Value = request.Email ?? "";
            command.Parameters.Add(emailParam);

            var passwordParam = command.CreateParameter();
            passwordParam.ParameterName = "@password";
            passwordParam.Value = request.Password ?? "";
            command.Parameters.Add(passwordParam);

            var firstNameParam = command.CreateParameter();
            firstNameParam.ParameterName = "@firstName";
            firstNameParam.Value = request.FirstName ?? "";
            command.Parameters.Add(firstNameParam);

            var lastNameParam = command.CreateParameter();
            lastNameParam.ParameterName = "@lastName";
            lastNameParam.Value = request.LastName ?? "";
            command.Parameters.Add(lastNameParam);

            var patronymicParam = command.CreateParameter();
            patronymicParam.ParameterName = "@patronymic";
            patronymicParam.Value = request.Patronymic ?? (object)DBNull.Value;
            command.Parameters.Add(patronymicParam);

            var phoneParam = command.CreateParameter();
            phoneParam.ParameterName = "@phone";
            phoneParam.Value = request.Phone ?? (object)DBNull.Value;
            command.Parameters.Add(phoneParam);

            newResidentId = (int)(await command.ExecuteScalarAsync() ?? 0);
        }

        // Set role if provided
        if (!string.IsNullOrEmpty(request.Role) && request.Role != "User")
        {
            int roleId = 1; // Default to User
            if (request.Role == "Admin")
            {
                roleId = 3;
            }
            else if (request.Role == "Moderator")
            {
                roleId = 2;
            }

            using (var roleCommand = connection.CreateCommand())
            {
                roleCommand.CommandText = "INSERT INTO ResidentRoles (ResidentId, RoleId) VALUES (@id, @roleId)";
                var idParam = roleCommand.CreateParameter();
                idParam.ParameterName = "@id";
                idParam.Value = newResidentId;
                roleCommand.Parameters.Add(idParam);

                var roleIdParam = roleCommand.CreateParameter();
                roleIdParam.ParameterName = "@roleId";
                roleIdParam.Value = roleId;
                roleCommand.Parameters.Add(roleIdParam);

                await roleCommand.ExecuteNonQueryAsync();
            }
        }

        // Return created user
        return CreatedAtAction(nameof(GetUser), new { id = newResidentId.ToString() }, new
        {
            Id = newResidentId.ToString(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Patronymic = request.Patronymic,
            Phone = request.Phone,
            Role = request.Role ?? "User",
            IsActive = true
        });
    }

    [HttpGet("residents")]
    public async Task<IActionResult> GetResidents()
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var residents = new List<object>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT ResidentId, FirstName, LastName, Patronymic, Email
                FROM Residents
                ORDER BY LastName, FirstName";

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    residents.Add(new
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.IsDBNull(1) ? "" : (reader.GetValue(1)?.ToString() ?? ""),
                        LastName = reader.IsDBNull(2) ? "" : (reader.GetValue(2)?.ToString() ?? ""),
                        Patronymic = reader.IsDBNull(3) ? "" : (reader.GetValue(3)?.ToString() ?? ""),
                        Email = reader.IsDBNull(4) ? "" : (reader.GetValue(4)?.ToString() ?? "")
                    });
                }
            }
        }

        return Ok(residents);
    }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class UpdateRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = "User";
}
