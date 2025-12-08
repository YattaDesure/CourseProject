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
    public Task<IActionResult> CreateParkingSpace([FromBody] dynamic space)
    {
        return Task.FromResult<IActionResult>(BadRequest(new { message = "Create functionality requires database schema update" }));
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
    public Task<IActionResult> DeleteParkingSpace(int id)
    {
        return Task.FromResult<IActionResult>(BadRequest(new { message = "Delete functionality requires database schema update" }));
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

    [HttpPost("import/excel")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Файл не выбран" });
        }

        if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
        {
            return BadRequest(new { message = "Поддерживаются только файлы Excel (.xlsx, .xls)" });
        }

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet == null)
            {
                return BadRequest(new { message = "Файл не содержит данных" });
            }

            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            int imported = 0;
            int errors = 0;
            var errorMessages = new List<string>();

            for (int row = 2; row <= worksheet.Dimension?.End.Row; row++)
            {
                try
                {
                    var number = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var areaStr = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var ownerEmail = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(number))
                    {
                        continue;
                    }

                    int? ownerId = null;
                    if (!string.IsNullOrEmpty(ownerEmail))
                    {
                        using (var findCommand = connection.CreateCommand())
                        {
                            findCommand.CommandText = "SELECT ResidentId FROM Residents WHERE Email = @email";
                            var emailParam = findCommand.CreateParameter();
                            emailParam.ParameterName = "@email";
                            emailParam.Value = ownerEmail;
                            findCommand.Parameters.Add(emailParam);

                            var result = await findCommand.ExecuteScalarAsync();
                            if (result != null && result != DBNull.Value)
                            {
                                ownerId = Convert.ToInt32(result);
                            }
                        }
                    }

                    decimal area = 0;
                    if (!string.IsNullOrEmpty(areaStr) && decimal.TryParse(areaStr, out var a))
                    {
                        area = a;
                    }

                    using (var checkCommand = connection.CreateCommand())
                    {
                        checkCommand.CommandText = "SELECT COUNT(*) FROM ParkingRooms WHERE Number = @number";
                        var numberParam = checkCommand.CreateParameter();
                        numberParam.ParameterName = "@number";
                        numberParam.Value = number;
                        checkCommand.Parameters.Add(numberParam);

                        var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                        if (exists > 0)
                        {
                            using (var updateCommand = connection.CreateCommand())
                            {
                                updateCommand.CommandText = @"
                                    UPDATE ParkingRooms 
                                    SET Area = @area, OwnerId = @ownerId
                                    WHERE Number = @number";
                                
                                var numberParamUpdate = updateCommand.CreateParameter();
                                numberParamUpdate.ParameterName = "@number";
                                numberParamUpdate.Value = number;
                                updateCommand.Parameters.Add(numberParamUpdate);
                                
                                var areaParam = updateCommand.CreateParameter();
                                areaParam.ParameterName = "@area";
                                areaParam.Value = area;
                                updateCommand.Parameters.Add(areaParam);

                                var ownerIdParam = updateCommand.CreateParameter();
                                ownerIdParam.ParameterName = "@ownerId";
                                ownerIdParam.Value = ownerId.HasValue ? (object)ownerId.Value : DBNull.Value;
                                updateCommand.Parameters.Add(ownerIdParam);

                                await updateCommand.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            using (var insertCommand = connection.CreateCommand())
                            {
                                insertCommand.CommandText = @"
                                    INSERT INTO ParkingRooms (Number, Area, OwnerId)
                                    VALUES (@number, @area, @ownerId)";
                                
                                var numberParam2 = insertCommand.CreateParameter();
                                numberParam2.ParameterName = "@number";
                                numberParam2.Value = number;
                                insertCommand.Parameters.Add(numberParam2);

                                var areaParam = insertCommand.CreateParameter();
                                areaParam.ParameterName = "@area";
                                areaParam.Value = area;
                                insertCommand.Parameters.Add(areaParam);

                                var ownerIdParam = insertCommand.CreateParameter();
                                ownerIdParam.ParameterName = "@ownerId";
                                ownerIdParam.Value = ownerId.HasValue ? (object)ownerId.Value : DBNull.Value;
                                insertCommand.Parameters.Add(ownerIdParam);

                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }

                        imported++;
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    errorMessages.Add($"Строка {row}: {ex.Message}");
                }
            }

            return Ok(new 
            { 
                message = $"Импорт завершен. Импортировано: {imported}, Ошибок: {errors}",
                imported = imported,
                errors = errors,
                errorMessages = errorMessages.Take(10).ToList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при импорте файла", error = ex.Message });
        }
    }
}

public class UpdateParkingRequest
{
    public string? SlotNumber { get; set; }
    public decimal Area { get; set; }
    public int? OwnerId { get; set; }
}
