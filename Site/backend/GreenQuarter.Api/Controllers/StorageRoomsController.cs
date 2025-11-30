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

            // Users can only see Available storage rooms (no owner)
            if (userRole == "User")
            {
                sql += " AND s.OwnerId IS NULL";
            }

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
        var worksheet = package.Workbook.Worksheets.Add("Кладовые");

        worksheet.Cells[1, 1].Value = "Номер";
        worksheet.Cells[1, 2].Value = "Площадь";
        worksheet.Cells[1, 3].Value = "Статус";
        worksheet.Cells[1, 4].Value = "Владелец";
        worksheet.Cells[1, 5].Value = "Email владельца";

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
                    s.Number,
                    s.Area,
                    s.OwnerId,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM StorageRooms s
                LEFT JOIN Residents r ON s.OwnerId = r.ResidentId
                WHERE 1=1";

            if (userRole == "User")
            {
                sql += " AND s.OwnerId IS NULL";
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
            $"Кладовые_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
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
                    s.Number,
                    s.Area,
                    s.OwnerId,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM StorageRooms s
                LEFT JOIN Residents r ON s.OwnerId = r.ResidentId
                WHERE 1=1";

            if (userRole == "User")
            {
                sql += " AND s.OwnerId IS NULL";
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
        return File(bytes, "text/csv; charset=utf-8", $"Кладовые_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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

public class UpdateStorageRequest
{
    public string? Label { get; set; }
    public decimal Area { get; set; }
    public int? OwnerId { get; set; }
}
