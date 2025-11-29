using System.Data.Common;
using System.Security.Claims;
using GreenQuarter.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}
