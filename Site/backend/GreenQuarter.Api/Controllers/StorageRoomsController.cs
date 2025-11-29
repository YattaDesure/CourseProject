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
public class StorageRoomsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StorageRoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetStorageRooms([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? level)
    {
        var userRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        var residentId = User.Claims.FirstOrDefault(c => c.Type == "ResidentId")?.Value ?? 
                        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var storageRooms = new List<object>();

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
                    s.StorageRoomId as Id,
                    s.Number as Label,
                    s.Area,
                    s.OwnerId,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM StorageRooms s
                LEFT JOIN Residents r ON s.OwnerId = r.ResidentId
                WHERE 1=1";

            // For now, show all storage rooms to all users
            // TODO: Implement proper filtering when data is linked correctly

            if (!string.IsNullOrEmpty(search))
            {
                sql += " AND s.Number LIKE @search";
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
                    var storageOwnerId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                    var isOccupied = storageOwnerId.HasValue && storageOwnerId.Value > 0;
                    
                    storageRooms.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Label = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Level = "", // Not in existing schema
                        Area = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2)),
                        Status = isOccupied ? "Occupied" : "Available",
                        OwnerId = storageOwnerId,
                        CreatedAt = DateTime.UtcNow,
                        Users = new[]
                        {
                            new
                            {
                                FirstName = reader.IsDBNull(4) ? "" : (reader.GetValue(4)?.ToString() ?? ""),
                                LastName = reader.IsDBNull(5) ? "" : (reader.GetValue(5)?.ToString() ?? ""),
                                Email = reader.IsDBNull(7) ? "" : (reader.GetValue(7)?.ToString() ?? "")
                            }
                        }.Where(u => !string.IsNullOrEmpty(u.FirstName) || !string.IsNullOrEmpty(u.Email)).ToList()
                    });
                }
            }
        }

        return Ok(storageRooms);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStorageRoom(int id)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT 
                    s.StorageRoomId,
                    s.Number,
                    s.Area,
                    s.OwnerId,
                    r.FirstName,
                    r.LastName,
                    r.Email
                FROM StorageRooms s
                LEFT JOIN Residents r ON s.OwnerId = r.ResidentId
                WHERE s.StorageRoomId = @id";

            var param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = id;
            command.Parameters.Add(param);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var room = new
                    {
                        Id = reader.GetInt32(0),
                        Label = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Level = "",
                        Area = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2)),
                        Status = "Occupied",
                        Owner = new
                        {
                            FirstName = reader.IsDBNull(4) ? "" : (reader.GetValue(4)?.ToString() ?? ""),
                            LastName = reader.IsDBNull(5) ? "" : (reader.GetValue(5)?.ToString() ?? ""),
                            Email = reader.IsDBNull(6) ? "" : (reader.GetValue(6)?.ToString() ?? "")
                        }
                    };
                    return Ok(room);
                }
            }
        }

        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> CreateStorageRoom([FromBody] dynamic storageRoom)
    {
        return BadRequest(new { message = "Create functionality requires database schema update" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> UpdateStorageRoom(int id, [FromBody] UpdateStorageRequest request)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE StorageRooms 
                SET Number = @number,
                    Area = @area,
                    OwnerId = @ownerId
                WHERE StorageRoomId = @id";

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = id;
            command.Parameters.Add(idParam);

            var numberParam = command.CreateParameter();
            numberParam.ParameterName = "@number";
            numberParam.Value = request.Label ?? "";
            command.Parameters.Add(numberParam);

            var areaParam = command.CreateParameter();
            areaParam.ParameterName = "@area";
            areaParam.Value = request.Area;
            command.Parameters.Add(areaParam);

            var ownerIdParam = command.CreateParameter();
            ownerIdParam.ParameterName = "@ownerId";
            ownerIdParam.Value = request.OwnerId.HasValue ? (object)request.OwnerId.Value : DBNull.Value;
            command.Parameters.Add(ownerIdParam);

            await command.ExecuteNonQueryAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStorageRoom(int id)
    {
        return BadRequest(new { message = "Delete functionality requires database schema update" });
    }
}

public class UpdateStorageRequest
{
    public string? Label { get; set; }
    public decimal Area { get; set; }
    public int? OwnerId { get; set; }
}
