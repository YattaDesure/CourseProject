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
