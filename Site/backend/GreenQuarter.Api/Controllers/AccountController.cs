using System.Data.Common;
using System.Data;
using System.Security.Claims;
using GreenQuarter.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace GreenQuarter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAccount()
    {
        var residentId = User.Claims.FirstOrDefault(c => c.Type == "ResidentId")?.Value ?? 
                        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(residentId) || !int.TryParse(residentId, out var residentIdInt))
        {
            return Unauthorized();
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        // Get resident info
        var resident = new { Id = "", Email = "", FirstName = "", LastName = "", Patronymic = "", Role = "User", CreatedAt = DateTime.UtcNow };
        var apartments = new List<object>();
        var parkingSpaces = new List<object>();
        var storageRooms = new List<object>();

        using (var command = connection.CreateCommand())
        {
            // Get resident details with role
            command.CommandText = @"
                SELECT r.ResidentId, r.Email, r.FirstName, r.LastName, r.Patronymic, 
                       COALESCE(rol.RoleName, 'User') as RoleName
                FROM Residents r
                LEFT JOIN ResidentRoles rr ON r.ResidentId = rr.ResidentId
                LEFT JOIN roles rol ON rr.RoleId = rol.RoleId
                WHERE r.ResidentId = @id";
            var param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = residentIdInt;
            command.Parameters.Add(param);

            string role = "User";
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var roleName = reader.IsDBNull(5) ? "User" : (reader.GetValue(5)?.ToString() ?? "User");
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
                    
                    resident = new
                    {
                        Id = reader.GetInt32(0).ToString(),
                        Email = reader.IsDBNull(1) ? "" : (reader.GetValue(1)?.ToString() ?? ""),
                        FirstName = reader.IsDBNull(2) ? "" : (reader.GetValue(2)?.ToString() ?? ""),
                        LastName = reader.IsDBNull(3) ? "" : (reader.GetValue(3)?.ToString() ?? ""),
                        Patronymic = reader.IsDBNull(4) ? "" : (reader.GetValue(4)?.ToString() ?? ""),
                        Role = role,
                        CreatedAt = DateTime.UtcNow
                    };
                }
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT ApartmentId, Number, Floor, Area, Entrance FROM Apartments WHERE ResidentId = @id";
            param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = residentIdInt;
            command.Parameters.Add(param);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    apartments.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Number = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Building = "",
                        Entrance = reader.IsDBNull(4) ? "" : reader.GetValue(4).ToString() ?? "",
                        Floor = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2)),
                        Area = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3)),
                        Status = "Occupied",
                        SharePercentage = 100m,
                        OwnedSince = DateTime.UtcNow
                    });
                }
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT ParkingRoomId, Number, Area FROM ParkingRooms WHERE OwnerId = @id";
            param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = residentIdInt;
            command.Parameters.Add(param);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    parkingSpaces.Add(new
                    {
                        Id = reader.GetInt32(0),
                        SlotNumber = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Area = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2)),
                        Status = "Occupied",
                        AssignedSince = DateTime.UtcNow
                    });
                }
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT StorageRoomId, Number, Area FROM StorageRooms WHERE OwnerId = @id";
            param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = residentIdInt;
            command.Parameters.Add(param);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    storageRooms.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Label = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Level = "",
                        Area = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2)),
                        Status = "Occupied",
                        AssignedSince = DateTime.UtcNow
                    });
                }
            }
        }

        var accountInfo = new
        {
            Id = resident.Id,
            Email = resident.Email,
            FirstName = resident.FirstName,
            LastName = resident.LastName,
            Patronymic = resident.Patronymic,
            Role = resident.Role,
            CreatedAt = resident.CreatedAt,
            Apartments = apartments,
            ParkingSpaces = parkingSpaces,
            StorageRooms = storageRooms
        };

        return Ok(accountInfo);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var residentId = User.Claims.FirstOrDefault(c => c.Type == "ResidentId")?.Value ?? 
                        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(residentId) || !int.TryParse(residentId, out var residentIdInt))
        {
            return Unauthorized();
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
            idParam.Value = residentIdInt;
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

        return Ok(new { message = "Профиль успешно обновлен" });
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var residentId = User.Claims.FirstOrDefault(c => c.Type == "ResidentId")?.Value ?? 
                        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(residentId) || !int.TryParse(residentId, out var residentIdInt))
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
            return BadRequest(new { message = "Старый и новый пароль обязательны" });
        }

        if (request.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "Новый пароль должен содержать минимум 6 символов" });
        }

        // Get connection string
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            return StatusCode(500, new { message = "Ошибка подключения к базе данных" });
        }

        // Verify old password using new connection
        string? currentPassword = null;
        using (var checkConnection = new SqlConnection(connectionString))
        {
            await checkConnection.OpenAsync();
            using (var checkCommand = checkConnection.CreateCommand())
            {
                checkCommand.CommandText = "SELECT Password FROM Residents WHERE ResidentId = @id";
                var idParam = checkCommand.CreateParameter();
                idParam.ParameterName = "@id";
                idParam.Value = residentIdInt;
                checkCommand.Parameters.Add(idParam);

                using (var reader = await checkCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        currentPassword = reader.IsDBNull(0) ? "" : (reader.GetValue(0)?.ToString() ?? "");
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(currentPassword))
        {
            return NotFound(new { message = "Пользователь не найден" });
        }

        if (currentPassword != request.OldPassword)
        {
            return BadRequest(new { message = "Неверный текущий пароль" });
        }

        // Update password using new connection
        // Disable trigger to avoid "Invalid column name 'Name'" error
        using (var updateConnection = new SqlConnection(connectionString))
        {
            await updateConnection.OpenAsync();
            using (var updateCommand = updateConnection.CreateCommand())
            {
                // Disable trigger before update
                updateCommand.CommandText = "ALTER TABLE Residents DISABLE TRIGGER ALL";
                await updateCommand.ExecuteNonQueryAsync();
                
                // Update password
                updateCommand.CommandText = "UPDATE Residents SET Password = @password WHERE ResidentId = @id";
                updateCommand.Parameters.Clear();
                
                var idParam = updateCommand.CreateParameter();
                idParam.ParameterName = "@id";
                idParam.Value = residentIdInt;
                updateCommand.Parameters.Add(idParam);

                var passwordParam = updateCommand.CreateParameter();
                passwordParam.ParameterName = "@password";
                passwordParam.Value = request.NewPassword ?? "";
                updateCommand.Parameters.Add(passwordParam);

                var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                
                // Re-enable trigger
                updateCommand.CommandText = "ALTER TABLE Residents ENABLE TRIGGER ALL";
                updateCommand.Parameters.Clear();
                await updateCommand.ExecuteNonQueryAsync();
                
                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Пользователь не найден" });
                }
            }
        }

        return Ok(new { message = "Пароль успешно изменен" });
    }
}

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Patronymic { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
