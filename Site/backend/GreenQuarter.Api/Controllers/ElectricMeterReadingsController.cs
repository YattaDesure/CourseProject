using System.Data.Common;
using GreenQuarter.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenQuarter.Api.Controllers;

[ApiController]
[Route("api/electric-meters")]
[Authorize]
public class ElectricMeterReadingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ElectricMeterReadingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("by-object")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetMeterByObject([FromQuery] string objectType, [FromQuery] int objectId)
    {
        var normalizedType = NormalizeObjectType(objectType);
        if (normalizedType is null)
        {
            return BadRequest(new { message = "Invalid objectType. Use: apartment, storage, parking." });
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = normalizedType switch
        {
            ElectricMeterObjectType.Apartment => @"
                SELECT TOP 1 m.ElectricMeterId, m.SerialNumber, m.ApartmentId, m.StorageRoomId, m.ParkingRoomId
                FROM ElectricMeters m
                WHERE m.ApartmentId = @objectId",
            ElectricMeterObjectType.Storage => @"
                SELECT TOP 1 m.ElectricMeterId, m.SerialNumber, m.ApartmentId, m.StorageRoomId, m.ParkingRoomId
                FROM ElectricMeters m
                WHERE m.StorageRoomId = @objectId",
            ElectricMeterObjectType.Parking => @"
                SELECT TOP 1 m.ElectricMeterId, m.SerialNumber, m.ApartmentId, m.StorageRoomId, m.ParkingRoomId
                FROM ElectricMeters m
                WHERE m.ParkingRoomId = @objectId",
            _ => throw new InvalidOperationException("Unexpected object type")
        };

        var idParam = command.CreateParameter();
        idParam.ParameterName = "@objectId";
        idParam.Value = objectId;
        command.Parameters.Add(idParam);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return NotFound(new { message = "Meter not found for object." });
        }

        var meterId = reader.GetInt32(0);
        var serial = reader.IsDBNull(1) ? null : reader.GetValue(1)?.ToString();

        return Ok(new
        {
            ElectricMeterId = meterId,
            SerialNumber = serial,
            ObjectType = normalizedType.Value.ToString().ToLowerInvariant(),
            ObjectId = objectId
        });
    }

    [HttpGet("last-readings")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetLastReadings([FromQuery] string objectType, [FromQuery] string objectIds)
    {
        var normalizedType = NormalizeObjectType(objectType);
        if (normalizedType is null)
        {
            return BadRequest(new { message = "Invalid objectType. Use: apartment, storage, parking." });
        }

        if (string.IsNullOrWhiteSpace(objectIds))
        {
            return Ok(Array.Empty<object>());
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var objectColumn = normalizedType.Value switch
        {
            ElectricMeterObjectType.Apartment => "ApartmentId",
            ElectricMeterObjectType.Storage => "StorageRoomId",
            ElectricMeterObjectType.Parking => "ParkingRoomId",
            _ => throw new InvalidOperationException("Unexpected object type")
        };

        var result = new List<object>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $@"
                SELECT
                    m.{objectColumn} AS ObjectId,
                    m.ElectricMeterId,
                    lastR.ReadingMonth,
                    lastR.ReadingValue
                FROM ElectricMeters m
                OUTER APPLY (
                    SELECT TOP 1 r.ReadingMonth, r.ReadingValue
                    FROM ElectricMeterReadings r
                    WHERE r.ElectricMeterId = m.ElectricMeterId
                    ORDER BY r.ReadingMonth DESC
                ) lastR
                WHERE m.{objectColumn} IS NOT NULL
                  AND m.{objectColumn} IN (
                      SELECT TRY_CONVERT(INT, LTRIM(RTRIM([value])))
                      FROM STRING_SPLIT(@ids, ',')
                      WHERE TRY_CONVERT(INT, LTRIM(RTRIM([value]))) IS NOT NULL
                  );";

            var p = command.CreateParameter();
            p.ParameterName = "@ids";
            p.Value = objectIds;
            command.Parameters.Add(p);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var objId = reader.IsDBNull(0) ? (int?)null : Convert.ToInt32(reader.GetValue(0));
                if (!objId.HasValue) continue;

                result.Add(new
                {
                    ObjectId = objId.Value,
                    ElectricMeterId = reader.GetInt32(1),
                    LastMonth = reader.IsDBNull(2) ? null : reader.GetDateTime(2).ToString("yyyy-MM-01"),
                    LastValue = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3)
                });
            }
        }

        return Ok(result);
    }

    [HttpGet("month-status")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetMonthStatus([FromQuery] string objectType, [FromQuery] string objectIds, [FromQuery] string readingMonth)
    {
        var normalizedType = NormalizeObjectType(objectType);
        if (normalizedType is null)
        {
            return BadRequest(new { message = "Invalid objectType. Use: apartment, storage, parking." });
        }

        if (string.IsNullOrWhiteSpace(objectIds))
        {
            return Ok(Array.Empty<object>());
        }

        var month = NormalizeMonth(readingMonth);
        if (month is null)
        {
            return BadRequest(new { message = "Invalid readingMonth. Use ISO date like 2026-05-01 or 2026-05." });
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var objectColumn = normalizedType.Value switch
        {
            ElectricMeterObjectType.Apartment => "ApartmentId",
            ElectricMeterObjectType.Storage => "StorageRoomId",
            ElectricMeterObjectType.Parking => "ParkingRoomId",
            _ => throw new InvalidOperationException("Unexpected object type")
        };

        var result = new List<object>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $@"
                SELECT
                    m.{objectColumn} AS ObjectId,
                    m.ElectricMeterId,
                    lastR.ReadingMonth AS LastMonth,
                    lastR.ReadingValue AS LastValue,
                    curR.ReadingValue AS CurrentMonthValue
                FROM ElectricMeters m
                OUTER APPLY (
                    SELECT TOP 1 r.ReadingMonth, r.ReadingValue
                    FROM ElectricMeterReadings r
                    WHERE r.ElectricMeterId = m.ElectricMeterId
                    ORDER BY r.ReadingMonth DESC
                ) lastR
                LEFT JOIN ElectricMeterReadings curR
                  ON curR.ElectricMeterId = m.ElectricMeterId
                 AND curR.ReadingMonth = @month
                WHERE m.{objectColumn} IS NOT NULL
                  AND m.{objectColumn} IN (
                      SELECT TRY_CONVERT(INT, LTRIM(RTRIM([value])))
                      FROM STRING_SPLIT(@ids, ',')
                      WHERE TRY_CONVERT(INT, LTRIM(RTRIM([value]))) IS NOT NULL
                  );";

            var pIds = command.CreateParameter();
            pIds.ParameterName = "@ids";
            pIds.Value = objectIds;
            command.Parameters.Add(pIds);

            var pMonth = command.CreateParameter();
            pMonth.ParameterName = "@month";
            pMonth.Value = month.Value.Date;
            command.Parameters.Add(pMonth);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var objId = reader.IsDBNull(0) ? (int?)null : Convert.ToInt32(reader.GetValue(0));
                if (!objId.HasValue) continue;

                result.Add(new
                {
                    ObjectId = objId.Value,
                    ElectricMeterId = reader.GetInt32(1),
                    LastMonth = reader.IsDBNull(2) ? null : reader.GetDateTime(2).ToString("yyyy-MM-01"),
                    LastValue = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3),
                    HasReadingForMonth = !reader.IsDBNull(4)
                });
            }
        }

        return Ok(result);
    }

    [HttpGet("missing-count")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetMissingCount([FromQuery] string readingMonth)
    {
        var month = NormalizeMonth(readingMonth);
        if (month is null)
        {
            return BadRequest(new { message = "Invalid readingMonth. Use ISO date like 2026-05-01 or 2026-05." });
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
              SUM(CASE WHEN m.ApartmentId IS NOT NULL AND r.ElectricMeterId IS NULL THEN 1 ELSE 0 END) AS MissingApartments,
              SUM(CASE WHEN m.StorageRoomId IS NOT NULL AND r.ElectricMeterId IS NULL THEN 1 ELSE 0 END) AS MissingStorage,
              SUM(CASE WHEN m.ParkingRoomId IS NOT NULL AND r.ElectricMeterId IS NULL THEN 1 ELSE 0 END) AS MissingParking,
              SUM(CASE WHEN r.ElectricMeterId IS NULL THEN 1 ELSE 0 END) AS MissingTotal
            FROM ElectricMeters m
            LEFT JOIN ElectricMeterReadings r
              ON r.ElectricMeterId = m.ElectricMeterId
             AND r.ReadingMonth = @month;";

        var p = command.CreateParameter();
        p.ParameterName = "@month";
        p.Value = month.Value.Date;
        command.Parameters.Add(p);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return Ok(new
            {
                Month = month.Value.ToString("yyyy-MM-01"),
                MissingTotal = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                MissingApartments = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                MissingStorage = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                MissingParking = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
            });
        }

        return Ok(new { Month = month.Value.ToString("yyyy-MM-01"), MissingTotal = 0, MissingApartments = 0, MissingStorage = 0, MissingParking = 0 });
    }

    [HttpGet("{electricMeterId:int}/readings")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetReadings(int electricMeterId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var readings = new List<object>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT ReadingMonth, ReadingValue, CreatedAt
                FROM ElectricMeterReadings
                WHERE ElectricMeterId = @meterId
                ORDER BY ReadingMonth DESC";

            var p = command.CreateParameter();
            p.ParameterName = "@meterId";
            p.Value = electricMeterId;
            command.Parameters.Add(p);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                readings.Add(new
                {
                    Month = reader.GetDateTime(0).ToString("yyyy-MM-01"),
                    Value = reader.GetDecimal(1),
                    CreatedAt = reader.GetDateTime(2)
                });
            }
        }

        return Ok(readings);
    }

    [HttpPost("readings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddReading([FromBody] AddElectricMeterReadingRequest request)
    {
        var normalizedType = NormalizeObjectType(request.ObjectType);
        if (normalizedType is null)
        {
            return BadRequest(new { message = "Invalid objectType. Use: apartment, storage, parking." });
        }

        if (request.ReadingValue < 0)
        {
            return BadRequest(new { message = "ReadingValue must be non-negative." });
        }

        var month = NormalizeMonth(request.ReadingMonth);
        if (month is null)
        {
            return BadRequest(new { message = "Invalid readingMonth. Use ISO date like 2026-05-01 or 2026-05." });
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var tx = await connection.BeginTransactionAsync();
        try
        {
            var meterId = await GetOrCreateMeterId(connection, tx, normalizedType.Value, request.ObjectId);
            if (meterId is null)
            {
                return NotFound(new { message = "Object not found." });
            }

            var lastValue = await GetLastReadingValue(connection, tx, meterId.Value);
            if (lastValue.HasValue && request.ReadingValue < lastValue.Value)
            {
                return BadRequest(new
                {
                    message = "ReadingValue cannot be less than previous reading.",
                    previous = lastValue.Value
                });
            }

            var inserted = await TryInsertReading(connection, tx, meterId.Value, month.Value, request.ReadingValue);
            if (!inserted)
            {
                return Conflict(new { message = "Reading for this month already exists." });
            }

            await tx.CommitAsync();
            return Ok(new
            {
                ElectricMeterId = meterId.Value,
                Month = month.Value.ToString("yyyy-MM-01"),
                Value = request.ReadingValue
            });
        }
        catch (DbException ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { message = "Database error", detail = ex.Message });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { message = "Server error", detail = ex.Message });
        }
    }

    private static async Task<decimal?> GetLastReadingValue(DbConnection connection, DbTransaction tx, int meterId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = @"
            SELECT TOP 1 ReadingValue
            FROM ElectricMeterReadings
            WHERE ElectricMeterId = @meterId
            ORDER BY ReadingMonth DESC";

        var p = command.CreateParameter();
        p.ParameterName = "@meterId";
        p.Value = meterId;
        command.Parameters.Add(p);

        var result = await command.ExecuteScalarAsync();
        if (result is null || result == DBNull.Value) return null;
        return Convert.ToDecimal(result);
    }

    private static async Task<bool> TryInsertReading(DbConnection connection, DbTransaction tx, int meterId, DateTime month, decimal value)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = @"
            INSERT INTO ElectricMeterReadings (ElectricMeterId, ReadingMonth, ReadingValue)
            SELECT @meterId, @month, @value
            WHERE NOT EXISTS (
                SELECT 1
                FROM ElectricMeterReadings
                WHERE ElectricMeterId = @meterId AND ReadingMonth = @month
            );";

        var p1 = command.CreateParameter();
        p1.ParameterName = "@meterId";
        p1.Value = meterId;
        command.Parameters.Add(p1);

        var p2 = command.CreateParameter();
        p2.ParameterName = "@month";
        p2.Value = month.Date;
        command.Parameters.Add(p2);

        var p3 = command.CreateParameter();
        p3.ParameterName = "@value";
        p3.Value = value;
        command.Parameters.Add(p3);

        var affected = await command.ExecuteNonQueryAsync();
        return affected > 0;
    }

    private static async Task<int?> GetOrCreateMeterId(DbConnection connection, DbTransaction tx, ElectricMeterObjectType type, int objectId)
    {
        // Ensure object exists and meter row exists (backfill usually does this, but keep endpoint resilient)
        var (objectTable, objectPk, meterObjectColumn) = type switch
        {
            ElectricMeterObjectType.Apartment => ("Apartments", "ApartmentId", "ApartmentId"),
            ElectricMeterObjectType.Storage => ("StorageRooms", "StorageRoomId", "StorageRoomId"),
            ElectricMeterObjectType.Parking => ("ParkingRooms", "ParkingRoomId", "ParkingRoomId"),
            _ => throw new InvalidOperationException("Unexpected object type")
        };

        // 1) Check object exists
        using (var exists = connection.CreateCommand())
        {
            exists.Transaction = tx;
            exists.CommandText = $"SELECT COUNT(1) FROM {objectTable} WHERE {objectPk} = @id";
            var p = exists.CreateParameter();
            p.ParameterName = "@id";
            p.Value = objectId;
            exists.Parameters.Add(p);
            var count = Convert.ToInt32(await exists.ExecuteScalarAsync());
            if (count <= 0) return null;
        }

        // 2) Try get existing meter
        using (var get = connection.CreateCommand())
        {
            get.Transaction = tx;
            get.CommandText = $"SELECT TOP 1 ElectricMeterId FROM ElectricMeters WHERE {meterObjectColumn} = @id";
            var p = get.CreateParameter();
            p.ParameterName = "@id";
            p.Value = objectId;
            get.Parameters.Add(p);

            var existing = await get.ExecuteScalarAsync();
            if (existing != null && existing != DBNull.Value)
            {
                return Convert.ToInt32(existing);
            }
        }

        // 3) Insert meter
        using (var insert = connection.CreateCommand())
        {
            insert.Transaction = tx;
            insert.CommandText = $@"
                INSERT INTO ElectricMeters ({meterObjectColumn})
                VALUES (@id);
                SELECT SCOPE_IDENTITY();";

            var p = insert.CreateParameter();
            p.ParameterName = "@id";
            p.Value = objectId;
            insert.Parameters.Add(p);

            var insertedId = await insert.ExecuteScalarAsync();
            return insertedId is null || insertedId == DBNull.Value ? null : Convert.ToInt32(insertedId);
        }
    }

    private static DateTime? NormalizeMonth(string readingMonth)
    {
        if (string.IsNullOrWhiteSpace(readingMonth)) return null;

        // Accept "YYYY-MM" or "YYYY-MM-DD"
        if (DateTime.TryParse(readingMonth, out var dt))
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        if (readingMonth.Length == 7 && readingMonth[4] == '-')
        {
            if (int.TryParse(readingMonth[..4], out var y) && int.TryParse(readingMonth[5..], out var m))
            {
                if (y is >= 1900 and <= 2100 && m is >= 1 and <= 12)
                {
                    return new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
                }
            }
        }

        return null;
    }

    private static ElectricMeterObjectType? NormalizeObjectType(string objectType)
    {
        if (string.IsNullOrWhiteSpace(objectType)) return null;
        var t = objectType.Trim().ToLowerInvariant();
        return t switch
        {
            "apartment" or "apartments" or "квартира" or "квартиры" => ElectricMeterObjectType.Apartment,
            "storage" or "storageroom" or "storagerooms" or "кладовая" or "кладовые" => ElectricMeterObjectType.Storage,
            "parking" or "parkingroom" or "parkingrooms" or "паркинг" or "парковка" => ElectricMeterObjectType.Parking,
            _ => null
        };
    }

    private enum ElectricMeterObjectType
    {
        Apartment = 1,
        Storage = 2,
        Parking = 3
    }
}

public sealed class AddElectricMeterReadingRequest
{
    public string ObjectType { get; set; } = "";
    public int ObjectId { get; set; }
    public string ReadingMonth { get; set; } = "";
    public decimal ReadingValue { get; set; }
}

