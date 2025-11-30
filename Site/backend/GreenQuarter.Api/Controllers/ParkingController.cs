using System.Data.Common;
using System.Security.Claims;
using System.Text;
using GreenQuarter.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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

    [HttpGet("export/excel")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> ExportToExcel()
    {
        var userRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Паркинг");

        worksheet.Cells[1, 1].Value = "Номер";
        worksheet.Cells[1, 2].Value = "Площадь";
        worksheet.Cells[1, 3].Value = "Статус";
        worksheet.Cells[1, 4].Value = "Владелец";
        worksheet.Cells[1, 5].Value = "Email владельца";

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
                    p.Number,
                    p.Area,
                    p.OwnerId,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM ParkingRooms p
                LEFT JOIN Residents r ON p.OwnerId = r.ResidentId
                WHERE 1=1";

            if (userRole == "User")
            {
                sql += " AND p.OwnerId IS NULL";
            }

            command.CommandText = sql;

            int row = 2;
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var ownerId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
                    var status = ownerId.HasValue && ownerId.Value > 0 ? "Занято" : "Свободно";
                    var ownerName = "";
                    if (!reader.IsDBNull(2))
                    {
                        var parts = new List<string>();
                        if (!reader.IsDBNull(3)) parts.Add(reader.GetValue(3)?.ToString() ?? "");
                        if (!reader.IsDBNull(4)) parts.Add(reader.GetValue(4)?.ToString() ?? "");
                        if (!reader.IsDBNull(5)) parts.Add(reader.GetValue(5)?.ToString() ?? "");
                        ownerName = string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
                    }

                    worksheet.Cells[row, 1].Value = reader.IsDBNull(0) ? "" : reader.GetValue(0)?.ToString() ?? "";
                    worksheet.Cells[row, 2].Value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                    worksheet.Cells[row, 3].Value = status;
                    worksheet.Cells[row, 4].Value = ownerName;
                    worksheet.Cells[row, 5].Value = reader.IsDBNull(6) ? "" : reader.GetValue(6)?.ToString() ?? "";
                    row++;
                }
            }
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Паркинг_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpGet("export/csv")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> ExportToCsv()
    {
        var userRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var csv = new StringBuilder();
        csv.AppendLine("Номер;Площадь;Статус;Владелец;Email владельца");

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
                    p.Number,
                    p.Area,
                    p.OwnerId,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM ParkingRooms p
                LEFT JOIN Residents r ON p.OwnerId = r.ResidentId
                WHERE 1=1";

            if (userRole == "User")
            {
                sql += " AND p.OwnerId IS NULL";
            }

            command.CommandText = sql;

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var ownerId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
                    var status = ownerId.HasValue && ownerId.Value > 0 ? "Занято" : "Свободно";
                    var ownerName = "";
                    if (!reader.IsDBNull(2))
                    {
                        var parts = new List<string>();
                        if (!reader.IsDBNull(3)) parts.Add(reader.GetValue(3)?.ToString() ?? "");
                        if (!reader.IsDBNull(4)) parts.Add(reader.GetValue(4)?.ToString() ?? "");
                        if (!reader.IsDBNull(5)) parts.Add(reader.GetValue(5)?.ToString() ?? "");
                        ownerName = string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
                    }

                    csv.AppendLine($"{EscapeCsv(reader.IsDBNull(0) ? "" : reader.GetValue(0)?.ToString() ?? "")};" +
                        $"{(reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1)))};" +
                        $"{EscapeCsv(status)};" +
                        $"{EscapeCsv(ownerName)};" +
                        $"{EscapeCsv(reader.IsDBNull(6) ? "" : reader.GetValue(6)?.ToString() ?? "")}");
                }
            }
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"Паркинг_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}

public class UpdateParkingRequest
{
    public string? SlotNumber { get; set; }
    public decimal Area { get; set; }
    public int? OwnerId { get; set; }
}
