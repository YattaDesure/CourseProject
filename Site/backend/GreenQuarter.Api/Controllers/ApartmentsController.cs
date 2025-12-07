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
        var worksheet = package.Workbook.Worksheets.Add("Квартиры");

        // Headers
        worksheet.Cells[1, 1].Value = "Номер";
        worksheet.Cells[1, 2].Value = "Этаж";
        worksheet.Cells[1, 3].Value = "Площадь";
        worksheet.Cells[1, 4].Value = "Подъезд";
        worksheet.Cells[1, 5].Value = "Статус";
        worksheet.Cells[1, 6].Value = "Владелец";
        worksheet.Cells[1, 7].Value = "Email владельца";

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
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

            if (userRole == "User")
            {
                sql += " AND a.ResidentId IS NULL";
            }

            command.CommandText = sql;

            int row = 2;
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var residentId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4);
                    var status = residentId.HasValue && residentId.Value > 0 ? "Занята" : "Свободна";
                    var ownerName = "";
                    if (!reader.IsDBNull(5))
                    {
                        var parts = new List<string>();
                        if (!reader.IsDBNull(6)) parts.Add(reader.GetValue(6)?.ToString() ?? "");
                        if (!reader.IsDBNull(7)) parts.Add(reader.GetValue(7)?.ToString() ?? "");
                        if (!reader.IsDBNull(8)) parts.Add(reader.GetValue(8)?.ToString() ?? "");
                        ownerName = string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
                    }

                    worksheet.Cells[row, 1].Value = reader.IsDBNull(0) ? "" : reader.GetValue(0)?.ToString() ?? "";
                    worksheet.Cells[row, 2].Value = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
                    worksheet.Cells[row, 3].Value = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                    worksheet.Cells[row, 4].Value = reader.IsDBNull(3) ? "" : reader.GetValue(3)?.ToString() ?? "";
                    worksheet.Cells[row, 5].Value = status;
                    worksheet.Cells[row, 6].Value = ownerName;
                    worksheet.Cells[row, 7].Value = reader.IsDBNull(8) ? "" : reader.GetValue(8)?.ToString() ?? "";
                    row++;
                }
            }
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Квартиры_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
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
        csv.AppendLine("Номер;Этаж;Площадь;Подъезд;Статус;Владелец;Email владельца");

        using (var command = connection.CreateCommand())
        {
            var sql = @"
                SELECT 
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

            if (userRole == "User")
            {
                sql += " AND a.ResidentId IS NULL";
            }

            command.CommandText = sql;

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var residentId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4);
                    var status = residentId.HasValue && residentId.Value > 0 ? "Занята" : "Свободна";
                    var ownerName = "";
                    if (!reader.IsDBNull(5))
                    {
                        var parts = new List<string>();
                        if (!reader.IsDBNull(6)) parts.Add(reader.GetValue(6)?.ToString() ?? "");
                        if (!reader.IsDBNull(7)) parts.Add(reader.GetValue(7)?.ToString() ?? "");
                        if (!reader.IsDBNull(8)) parts.Add(reader.GetValue(8)?.ToString() ?? "");
                        ownerName = string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
                    }

                    csv.AppendLine($"{EscapeCsv(reader.IsDBNull(0) ? "" : reader.GetValue(0)?.ToString() ?? "")};" +
                        $"{(reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1)))};" +
                        $"{(reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2)))};" +
                        $"{EscapeCsv(reader.IsDBNull(3) ? "" : reader.GetValue(3)?.ToString() ?? "")};" +
                        $"{EscapeCsv(status)};" +
                        $"{EscapeCsv(ownerName)};" +
                        $"{EscapeCsv(reader.IsDBNull(8) ? "" : reader.GetValue(8)?.ToString() ?? "")}");
                }
            }
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"Квартиры_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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

            // Пропускаем заголовок (первая строка)
            for (int row = 2; row <= worksheet.Dimension?.End.Row; row++)
            {
                try
                {
                    var number = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var floorStr = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var areaStr = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var entrance = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                    var ownerEmail = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(number))
                    {
                        continue; // Пропускаем пустые строки
                    }

                    int? residentId = null;
                    if (!string.IsNullOrEmpty(ownerEmail))
                    {
                        // Найти ResidentId по email
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
                                residentId = Convert.ToInt32(result);
                            }
                        }
                    }

                    int floor = 0;
                    if (!string.IsNullOrEmpty(floorStr) && int.TryParse(floorStr, out var f))
                    {
                        floor = f;
                    }

                    decimal area = 0;
                    if (!string.IsNullOrEmpty(areaStr) && decimal.TryParse(areaStr, out var a))
                    {
                        area = a;
                    }

                    // Проверить, существует ли квартира с таким номером
                    using (var checkCommand = connection.CreateCommand())
                    {
                        checkCommand.CommandText = "SELECT COUNT(*) FROM Apartments WHERE Number = @number";
                        var numberParam = checkCommand.CreateParameter();
                        numberParam.ParameterName = "@number";
                        numberParam.Value = number;
                        checkCommand.Parameters.Add(numberParam);

                        var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                        if (exists > 0)
                        {
                            // Обновить существующую
                            using (var updateCommand = connection.CreateCommand())
                            {
                                updateCommand.CommandText = @"
                                    UPDATE Apartments 
                                    SET Floor = @floor, Area = @area, Entrance = @entrance, ResidentId = @residentId
                                    WHERE Number = @number";
                                
                                updateCommand.Parameters.Add(checkCommand.Parameters[0].Clone());
                                
                                var floorParam = updateCommand.CreateParameter();
                                floorParam.ParameterName = "@floor";
                                floorParam.Value = floor;
                                updateCommand.Parameters.Add(floorParam);

                                var areaParam = updateCommand.CreateParameter();
                                areaParam.ParameterName = "@area";
                                areaParam.Value = area;
                                updateCommand.Parameters.Add(areaParam);

                                var entranceParam = updateCommand.CreateParameter();
                                entranceParam.ParameterName = "@entrance";
                                entranceParam.Value = entrance ?? "";
                                updateCommand.Parameters.Add(entranceParam);

                                var residentIdParam = updateCommand.CreateParameter();
                                residentIdParam.ParameterName = "@residentId";
                                residentIdParam.Value = residentId.HasValue ? (object)residentId.Value : DBNull.Value;
                                updateCommand.Parameters.Add(residentIdParam);

                                await updateCommand.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            // Создать новую
                            using (var insertCommand = connection.CreateCommand())
                            {
                                insertCommand.CommandText = @"
                                    INSERT INTO Apartments (Number, Floor, Area, Entrance, ResidentId)
                                    VALUES (@number, @floor, @area, @entrance, @residentId)";
                                
                                var numberParam2 = insertCommand.CreateParameter();
                                numberParam2.ParameterName = "@number";
                                numberParam2.Value = number;
                                insertCommand.Parameters.Add(numberParam2);

                                var floorParam = insertCommand.CreateParameter();
                                floorParam.ParameterName = "@floor";
                                floorParam.Value = floor;
                                insertCommand.Parameters.Add(floorParam);

                                var areaParam = insertCommand.CreateParameter();
                                areaParam.ParameterName = "@area";
                                areaParam.Value = area;
                                insertCommand.Parameters.Add(areaParam);

                                var entranceParam = insertCommand.CreateParameter();
                                entranceParam.ParameterName = "@entrance";
                                entranceParam.Value = entrance ?? "";
                                insertCommand.Parameters.Add(entranceParam);

                                var residentIdParam = insertCommand.CreateParameter();
                                residentIdParam.ParameterName = "@residentId";
                                residentIdParam.Value = residentId.HasValue ? (object)residentId.Value : DBNull.Value;
                                insertCommand.Parameters.Add(residentIdParam);

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
                errorMessages = errorMessages.Take(10).ToList() // Первые 10 ошибок
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при импорте файла", error = ex.Message });
        }
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
