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
public class ApartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetApartments([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? building)
    {
        var userRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        var residentId = User.Claims.FirstOrDefault(c => c.Type == "ResidentId")?.Value ?? 
                        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var apartments = new List<object>();

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
                    a.ApartmentId as Id,
                    a.Number,
                    a.Floor,
                    a.Area,
                    a.Entrance,
                    a.ResidentId,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM Apartments a
                LEFT JOIN Residents r ON a.ResidentId = r.ResidentId
                WHERE 1=1";

            // Users can only see Available apartments (no owner)
            if (userRole == "User")
            {
                sql += " AND a.ResidentId IS NULL";
            }

            if (!string.IsNullOrEmpty(search))
            {
                sql += " AND (a.Number LIKE @search OR a.Entrance LIKE @search)";
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
                    var apartmentResidentId = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5);
                    var isOccupied = apartmentResidentId.HasValue && apartmentResidentId.Value > 0;
                    
                    apartments.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Number = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Floor = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2)),
                        Area = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3)),
                        Entrance = reader.IsDBNull(4) ? "" : reader.GetValue(4).ToString() ?? "",
                        Status = isOccupied ? "Occupied" : "Available",
                        ResidentId = apartmentResidentId,
                        CreatedAt = DateTime.UtcNow,
                        Owners = new[]
                        {
                            new
                            {
                                FirstName = reader.IsDBNull(6) ? "" : (reader.GetValue(6)?.ToString() ?? ""),
                                LastName = reader.IsDBNull(7) ? "" : (reader.GetValue(7)?.ToString() ?? ""),
                                Email = reader.IsDBNull(9) ? "" : (reader.GetValue(9)?.ToString() ?? ""),
                                SharePercentage = 100m
                            }
                        }.Where(o => !string.IsNullOrEmpty(o.FirstName) || !string.IsNullOrEmpty(o.Email)).ToList()
                    });
                }
            }
        }

        return Ok(apartments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetApartment(int id)
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
                    a.ApartmentId,
                    a.Number,
                    a.Floor,
                    a.Area,
                    a.Entrance,
                    a.ResidentId,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM Apartments a
                LEFT JOIN Residents r ON a.ResidentId = r.ResidentId
                WHERE a.ApartmentId = @id";

            var param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = id;
            command.Parameters.Add(param);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var apartment = new
                    {
                        Id = reader.GetInt32(0),
                        Number = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString() ?? "",
                        Floor = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2)),
                        Area = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3)),
                        Entrance = reader.IsDBNull(4) ? "" : reader.GetValue(4).ToString() ?? "",
                        Status = reader.IsDBNull(5) ? "Available" : "Occupied",
                        ResidentId = reader.IsDBNull(5) ? (int?)null : Convert.ToInt32(reader.GetValue(5)),
                        Owner = new
                        {
                            FirstName = reader.IsDBNull(6) ? "" : (reader.GetValue(6)?.ToString() ?? ""),
                            LastName = reader.IsDBNull(7) ? "" : (reader.GetValue(7)?.ToString() ?? ""),
                            Email = reader.IsDBNull(9) ? "" : (reader.GetValue(9)?.ToString() ?? "")
                        }
                    };
                    return Ok(apartment);
                }
            }
        }

        return NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> CreateApartment([FromBody] dynamic apartment)
    {
        // Implementation for creating apartment in existing table
        return BadRequest(new { message = "Create functionality requires database schema update" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> UpdateApartment(int id, [FromBody] UpdateApartmentRequest request)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                UPDATE Apartments 
                SET Number = @number,
                    Floor = @floor,
                    Area = @area,
                    Entrance = @entrance,
                    ResidentId = @residentId
                WHERE ApartmentId = @id";

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = id;
            command.Parameters.Add(idParam);

            var numberParam = command.CreateParameter();
            numberParam.ParameterName = "@number";
            numberParam.Value = request.Number ?? "";
            command.Parameters.Add(numberParam);

            var floorParam = command.CreateParameter();
            floorParam.ParameterName = "@floor";
            floorParam.Value = request.Floor;
            command.Parameters.Add(floorParam);

            var areaParam = command.CreateParameter();
            areaParam.ParameterName = "@area";
            areaParam.Value = request.Area;
            command.Parameters.Add(areaParam);

            var entranceParam = command.CreateParameter();
            entranceParam.ParameterName = "@entrance";
            entranceParam.Value = request.Entrance ?? "";
            command.Parameters.Add(entranceParam);

            var residentIdParam = command.CreateParameter();
            residentIdParam.ParameterName = "@residentId";
            residentIdParam.Value = request.ResidentId.HasValue ? (object)request.ResidentId.Value : DBNull.Value;
            command.Parameters.Add(residentIdParam);

            await command.ExecuteNonQueryAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteApartment(int id)
    {
        return BadRequest(new { message = "Delete functionality requires database schema update" });
    }
}

public class UpdateApartmentRequest
{
    public string? Number { get; set; }
    public int Floor { get; set; }
    public decimal Area { get; set; }
    public string? Entrance { get; set; }
    public int? ResidentId { get; set; }
}
