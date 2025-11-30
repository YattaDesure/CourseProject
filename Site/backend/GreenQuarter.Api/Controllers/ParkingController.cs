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
public class ParkingController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ParkingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetParkingSpaces([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? section)
    {
        var userRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        var residentId = User.Claims.FirstOrDefault(c => c.Type == "ResidentId")?.Value ?? 
                        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var parkingSpaces = new List<object>();

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
                    p.ParkingRoomId as Id,
                    p.Number as SlotNumber,
                    p.Area,
                    p.OwnerId,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM ParkingRooms p
                LEFT JOIN Residents r ON p.OwnerId = r.ResidentId
                WHERE 1=1";

            // Users can only see Available parking spaces (no owner)
            if (userRole == "User")
            {
                sql += " AND p.OwnerId IS NULL";
            }

            if (!string.IsNullOrEmpty(search))
            {
                sql += " AND p.Number LIKE @search";
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
                    var ownerId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                    var hasOwner = ownerId.HasValue && ownerId.Value > 0;
                    
                    parkingSpaces.Add(new
                    {
                        Id = reader.GetInt32(0),
                        SlotNumber = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Section = "", // Not in existing schema
                        Size = "Standard",
                        IsCovered = false,
                        IsEVReady = false,
                        Status = hasOwner ? "Occupied" : "Available",
                        Area = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2)),
                        OwnerId = ownerId,
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

        return Ok(parkingSpaces);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetParkingSpace(int id)
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
                    p.ParkingRoomId,
                    p.Number,
                    p.Area,
                    p.OwnerId,
                    r.FirstName,
                    r.LastName,
                    r.Email
                FROM ParkingRooms p
                LEFT JOIN Residents r ON p.OwnerId = r.ResidentId
                WHERE p.ParkingRoomId = @id";

            var param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = id;
            command.Parameters.Add(param);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var space = new
                    {
                        Id = reader.GetInt32(0),
                        SlotNumber = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Section = "",
                        Size = "Standard",
                        IsCovered = false,
                        IsEVReady = false,
                        Status = "Occupied",
                        Area = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2)),
                        Owner = new
                        {
                            FirstName = reader.IsDBNull(4) ? "" : (reader.GetValue(4)?.ToString() ?? ""),
                            LastName = reader.IsDBNull(5) ? "" : (reader.GetValue(5)?.ToString() ?? ""),
                            Email = reader.IsDBNull(6) ? "" : (reader.GetValue(6)?.ToString() ?? "")
                        }
                    };
                    return Ok(space);
                }
            }
        }

        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> CreateParkingSpace([FromBody] dynamic space)
    {
        return BadRequest(new { message = "Create functionality requires database schema update" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> UpdateParkingSpace(int id, [FromBody] UpdateParkingRequest request)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE ParkingRooms 
                SET Number = @number,
                    Area = @area,
                    OwnerId = @ownerId
                WHERE ParkingRoomId = @id";

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = id;
            command.Parameters.Add(idParam);

            var numberParam = command.CreateParameter();
            numberParam.ParameterName = "@number";
            numberParam.Value = request.SlotNumber ?? "";
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
    public async Task<IActionResult> DeleteParkingSpace(int id)
    {
        return BadRequest(new { message = "Delete functionality requires database schema update" });
    }
}

public class UpdateParkingRequest
{
    public string? SlotNumber { get; set; }
    public decimal Area { get; set; }
    public int? OwnerId { get; set; }
}
