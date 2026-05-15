using System.Data.Common;
using GreenQuarter.Api.Infrastructure;
using GreenQuarter.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenQuarter.Api.Controllers;

/// <summary>
/// Квитанции: формирование начислений (Admin) и просмотр/демо-оплата (жилец).
/// Капремонт — по площади; электричество — по разнице показаний счётчиков.
/// </summary>
[ApiController]
[Route("api/receipts")]
[Authorize]
public sealed class ReceiptsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReceiptsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Сформировать или пересчитать квитанции за месяц для всех жильцов с объектами.</summary>
    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Generate([FromQuery] string billingMonth)
    {
        var month = NormalizeMonth(billingMonth);
        if (month is null)
        {
            return BadRequest(new { message = "Invalid billingMonth. Use 2026-05 or 2026-05-01." });
        }

        var capRate = await GetServiceRateAsync("CapRepair", month.Value.Date);
        var elecRate = await GetServiceRateAsync("Electricity", month.Value.Date);
        var connection = await DbMini.OpenAsync(_context);

        using var tx = await connection.BeginTransactionAsync();
        try
        {
            var residents = await GetResidentsWithAnyObjectsAsync(connection, tx);

            var created = 0;
            var updated = 0;

            foreach (var residentId in residents)
            {
                var receiptId = await GetExistingReceiptIdAsync(connection, tx, residentId, month.Value.Date);
                if (receiptId.HasValue)
                {
                    await DeleteReceiptLinesAsync(connection, tx, receiptId.Value);
                    updated++;
                }
                else
                {
                    receiptId = await InsertReceiptAsync(connection, tx, residentId, month.Value.Date);
                    created++;
                }

                var total = 0m;

                var capLines = await BuildCapRepairLinesAsync(connection, tx, residentId, receiptId.Value, capRate);
                foreach (var line in capLines)
                {
                    await InsertReceiptLineAsync(connection, tx, line);
                    total += line.Amount;
                }

                var elecLines = await BuildElectricityLinesAsync(connection, tx, residentId, receiptId.Value, elecRate, month.Value.Date);
                foreach (var line in elecLines)
                {
                    await InsertReceiptLineAsync(connection, tx, line);
                    total += line.Amount;
                }

                await UpdateReceiptTotalAsync(connection, tx, receiptId.Value, total);
                await SetReceiptPaymentMetaAsync(connection, tx, receiptId.Value, month.Value.Date, resetPaid: true);
            }

            await tx.CommitAsync();
            return Ok(new
            {
                billingMonth = month.Value.ToString("yyyy-MM-01"),
                capRepairRatePerSqm = capRate,
                electricityRatePerKwh = elecRate,
                created,
                updated,
                residents = residents.Count
            });
        }
        catch (DbException ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { message = "Database error", detail = ex.Message });
        }
    }

    /// <summary>Список квитанций текущего жильца (с полями оплаты).</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] int limit = 12)
    {
        var residentId = await ResidentIdFromUser.ResolveAsync(User, _context);
        if (!residentId.HasValue) return Unauthorized();

        if (limit <= 0) limit = 12;
        if (limit > 36) limit = 36;

        var connection = await DbMini.OpenAsync(_context);
        var items = new List<object>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TOP (@limit)
                ReceiptId,
                BillingMonth,
                TotalAmount,
                CreatedAt,
                PaymentStatus,
                PaymentDueDate,
                PaidAt,
                PaymentReference
            FROM Receipts
            WHERE ResidentId = @residentId
            ORDER BY BillingMonth DESC, ReceiptId DESC;";
        command.Parameters.Add(DbMini.P(command, "@limit", limit));
        command.Parameters.Add(DbMini.P(command, "@residentId", residentId.Value));

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(MapReceiptListRow(reader));
        }

        return Ok(items);
    }

    [HttpGet("my/{receiptId:int}")]
    public async Task<IActionResult> GetMyDetail([FromRoute] int receiptId)
    {
        var residentId = await ResidentIdFromUser.ResolveAsync(User, _context);
        if (!residentId.HasValue) return Unauthorized();

        var connection = await DbMini.OpenAsync(_context);

        int? dbResidentId = null;
        DateTime? billingMonth = null;
        decimal total = 0m;
        DateTime? createdAt = null;
        string paymentStatus = "Unpaid";
        DateTime? paymentDueDate = null;
        DateTime? paidAt = null;
        string? paymentReference = null;
        string payerName = "";
        string payerEmail = "";

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT
                    rec.ResidentId,
                    rec.BillingMonth,
                    rec.TotalAmount,
                    rec.CreatedAt,
                    rec.PaymentStatus,
                    rec.PaymentDueDate,
                    rec.PaidAt,
                    rec.PaymentReference,
                    r.FirstName,
                    r.LastName,
                    r.Patronymic,
                    r.Email
                FROM Receipts rec
                INNER JOIN Residents r ON r.ResidentId = rec.ResidentId
                WHERE rec.ReceiptId = @id;";
            command.Parameters.Add(DbMini.P(command, "@id", receiptId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dbResidentId = reader.GetInt32(0);
                billingMonth = reader.GetDateTime(1);
                total = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
                createdAt = reader.GetDateTime(3);
                paymentStatus = reader.IsDBNull(4) ? "Unpaid" : (reader.GetValue(4)?.ToString() ?? "Unpaid");
                paymentDueDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5);
                paidAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6);
                paymentReference = reader.IsDBNull(7) ? null : reader.GetValue(7)?.ToString();
                var fn = reader.IsDBNull(8) ? "" : (reader.GetValue(8)?.ToString() ?? "");
                var ln = reader.IsDBNull(9) ? "" : (reader.GetValue(9)?.ToString() ?? "");
                var pat = reader.IsDBNull(10) ? "" : (reader.GetValue(10)?.ToString() ?? "");
                payerName = $"{ln} {fn} {pat}".Trim();
                payerEmail = reader.IsDBNull(11) ? "" : (reader.GetValue(11)?.ToString() ?? "");
            }
        }

        if (!dbResidentId.HasValue) return NotFound();
        if (dbResidentId.Value != residentId.Value) return Forbid();

        var lines = new List<object>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT
                    ReceiptLineId,
                    ServiceCode,
                    ObjectType,
                    ObjectId,
                    AreaSqm,
                    RatePerSqm,
                    Amount
                FROM ReceiptLines
                WHERE ReceiptId = @id
                ORDER BY ServiceCode, ObjectType, ObjectId;";
            command.Parameters.Add(DbMini.P(command, "@id", receiptId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lines.Add(new
                {
                    ReceiptLineId = reader.GetInt32(0),
                    ServiceCode = reader.IsDBNull(1) ? "" : (reader.GetValue(1)?.ToString() ?? ""),
                    ObjectType = reader.IsDBNull(2) ? "" : (reader.GetValue(2)?.ToString() ?? ""),
                    ObjectId = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3)),
                    AreaSqm = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    RatePerSqm = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                    Amount = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6)
                });
            }
        }

        return Ok(new
        {
            ReceiptId = receiptId,
            BillingMonth = billingMonth!.Value.ToString("yyyy-MM-01"),
            TotalAmount = total,
            CreatedAt = createdAt,
            PaymentStatus = paymentStatus,
            PaymentDueDate = paymentDueDate?.ToString("yyyy-MM-dd"),
            PaidAt = paidAt,
            PaymentReference = paymentReference,
            PayerName = payerName,
            PayerEmail = payerEmail,
            Lines = lines
        });
    }

    /// <summary>Отметка «оплачено» в системе (без проверки банка).</summary>
    [HttpPost("my/{receiptId:int}/mark-paid")]
    public async Task<IActionResult> MarkMyReceiptPaid([FromRoute] int receiptId)
    {
        var residentId = await ResidentIdFromUser.ResolveAsync(User, _context);
        if (!residentId.HasValue) return Unauthorized();

        var connection = await DbMini.OpenAsync(_context);
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Receipts
            SET PaymentStatus = N'Paid',
                PaidAt = SYSUTCDATETIME()
            WHERE ReceiptId = @id AND ResidentId = @rid AND PaymentStatus <> N'Paid';
            SELECT @@ROWCOUNT;";
        command.Parameters.Add(DbMini.P(command, "@id", receiptId));
        command.Parameters.Add(DbMini.P(command, "@rid", residentId.Value));

        var rows = Convert.ToInt32(await command.ExecuteScalarAsync());
        if (rows == 0)
        {
            using var check = connection.CreateCommand();
            check.CommandText = "SELECT PaymentStatus FROM Receipts WHERE ReceiptId = @id AND ResidentId = @rid;";
            check.Parameters.Add(DbMini.P(check, "@id", receiptId));
            check.Parameters.Add(DbMini.P(check, "@rid", residentId.Value));
            var status = await check.ExecuteScalarAsync() as string;
            if (status is null) return NotFound();
            if (status == "Paid") return Ok(new { message = "Уже отмечено как оплачено", paymentStatus = "Paid" });
            return BadRequest(new { message = "Не удалось обновить статус" });
        }

        return Ok(new { message = "Квитанция отмечена как оплаченная", paymentStatus = "Paid" });
    }

    private static object MapReceiptListRow(System.Data.Common.DbDataReader reader)
    {
        return new
        {
            ReceiptId = reader.GetInt32(0),
            BillingMonth = reader.GetDateTime(1).ToString("yyyy-MM-01"),
            TotalAmount = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
            CreatedAt = reader.GetDateTime(3),
            PaymentStatus = reader.FieldCount > 4 && !reader.IsDBNull(4) ? reader.GetValue(4)?.ToString() ?? "Unpaid" : "Unpaid",
            PaymentDueDate = reader.FieldCount > 5 && !reader.IsDBNull(5) ? reader.GetDateTime(5).ToString("yyyy-MM-dd") : null,
            PaidAt = reader.FieldCount > 6 && !reader.IsDBNull(6) ? reader.GetDateTime(6) : (DateTime?)null,
            PaymentReference = reader.FieldCount > 7 && !reader.IsDBNull(7) ? reader.GetValue(7)?.ToString() : null
        };
    }

    /// <summary>Срок оплаты (25-е), референс GQ-{id}-{yyyyMM}; при пересчёте сбрасывает статус Unpaid.</summary>
    private static async Task SetReceiptPaymentMetaAsync(
        DbConnection connection,
        DbTransaction tx,
        int receiptId,
        DateTime billingMonth,
        bool resetPaid)
    {
        var due = new DateTime(billingMonth.Year, billingMonth.Month, 25);
        var reference = $"GQ-{receiptId}-{billingMonth:yyyyMM}";

        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = resetPaid
            ? @"
                UPDATE Receipts
                SET PaymentDueDate = @due,
                    PaymentReference = @ref,
                    PaymentStatus = N'Unpaid',
                    PaidAt = NULL
                WHERE ReceiptId = @id;"
            : @"
                UPDATE Receipts
                SET PaymentDueDate = @due,
                    PaymentReference = @ref
                WHERE ReceiptId = @id;";
        command.Parameters.Add(DbMini.P(command, "@due", due));
        command.Parameters.Add(DbMini.P(command, "@ref", reference));
        command.Parameters.Add(DbMini.P(command, "@id", receiptId));
        await command.ExecuteNonQueryAsync();
    }

    private static DateTime? NormalizeMonth(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var v = value.Trim();

        if (v.Length == 7 && DateTime.TryParse(v + "-01", out var ym))
        {
            return new DateTime(ym.Year, ym.Month, 1);
        }

        if (DateTime.TryParse(v, out var d))
        {
            return new DateTime(d.Year, d.Month, 1);
        }

        return null;
    }

    private async Task<decimal> GetServiceRateAsync(string serviceCode, DateTime billingMonth)
    {
        var connection = await DbMini.OpenAsync(_context);
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TOP 1 RatePerSqm
            FROM ServiceTariffs
            WHERE ServiceCode = @code
              AND ActiveFromMonth <= @m
              AND (ActiveToMonth IS NULL OR ActiveToMonth >= @m)
            ORDER BY ActiveFromMonth DESC, ServiceTariffId DESC;";
        command.Parameters.Add(DbMini.P(command, "@code", serviceCode));
        command.Parameters.Add(DbMini.P(command, "@m", billingMonth.Date));

        var scalar = await command.ExecuteScalarAsync();
        if (scalar is null || scalar == DBNull.Value)
        {
            // Капремонт: дефолт только если в БД ещё не завели тариф (обратная совместимость).
            // Электроэнергия и прочие услуги — только из dbo.ServiceTariffs; строк не будет, пока не добавите тариф в БД.
            return serviceCode == "CapRepair" ? 33.00m : 0m;
        }

        return Convert.ToDecimal(scalar);
    }

    private static async Task<List<int>> GetResidentsWithAnyObjectsAsync(DbConnection connection, DbTransaction tx)
    {
        var ids = new List<int>();
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = @"
            SELECT DISTINCT r.ResidentId
            FROM Residents r
            WHERE EXISTS (SELECT 1 FROM Apartments a WHERE a.ResidentId = r.ResidentId)
               OR EXISTS (SELECT 1 FROM StorageRooms s WHERE s.OwnerId = r.ResidentId)
               OR EXISTS (SELECT 1 FROM ParkingRooms p WHERE p.OwnerId = r.ResidentId);";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ids.Add(reader.GetInt32(0));
        }

        return ids;
    }

    private static async Task<int?> GetExistingReceiptIdAsync(DbConnection connection, DbTransaction tx, int residentId, DateTime billingMonth)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = @"
            SELECT TOP 1 ReceiptId
            FROM Receipts
            WHERE ResidentId = @r AND BillingMonth = @m;";
        command.Parameters.Add(DbMini.P(command, "@r", residentId));
        command.Parameters.Add(DbMini.P(command, "@m", billingMonth.Date));

        var scalar = await command.ExecuteScalarAsync();
        if (scalar is null || scalar == DBNull.Value) return null;
        return Convert.ToInt32(scalar);
    }

    private static async Task<int> InsertReceiptAsync(DbConnection connection, DbTransaction tx, int residentId, DateTime billingMonth)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = @"
            INSERT INTO Receipts (ResidentId, BillingMonth, TotalAmount)
            VALUES (@r, @m, 0);
            SELECT SCOPE_IDENTITY();";
        command.Parameters.Add(DbMini.P(command, "@r", residentId));
        command.Parameters.Add(DbMini.P(command, "@m", billingMonth.Date));

        var scalar = await command.ExecuteScalarAsync();
        return Convert.ToInt32(scalar);
    }

    private static async Task DeleteReceiptLinesAsync(DbConnection connection, DbTransaction tx, int receiptId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM ReceiptLines WHERE ReceiptId = @id;";
        command.Parameters.Add(DbMini.P(command, "@id", receiptId));
        await command.ExecuteNonQueryAsync();
    }

    private static async Task UpdateReceiptTotalAsync(DbConnection connection, DbTransaction tx, int receiptId, decimal total)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "UPDATE Receipts SET TotalAmount = @t WHERE ReceiptId = @id;";
        command.Parameters.Add(DbMini.P(command, "@t", total));
        command.Parameters.Add(DbMini.P(command, "@id", receiptId));
        await command.ExecuteNonQueryAsync();
    }

    private sealed record ReceiptLineToInsert(
        int ReceiptId,
        string ServiceCode,
        string ObjectType,
        int ObjectId,
        decimal AreaSqm,
        decimal RatePerSqm,
        decimal Amount
    );

    private static async Task<List<ReceiptLineToInsert>> BuildCapRepairLinesAsync(
        DbConnection connection,
        DbTransaction tx,
        int residentId,
        int receiptId,
        decimal ratePerSqm
    )
    {
        var lines = new List<ReceiptLineToInsert>();

        async Task AddFromAsync(string sql, string objectType)
        {
            using var command = connection.CreateCommand();
            command.Transaction = tx;
            command.CommandText = sql;
            command.Parameters.Add(DbMini.P(command, "@id", residentId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var objectId = reader.GetInt32(0);
                var area = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
                if (area < 0) area = 0m;
                var amount = Math.Round(area * ratePerSqm, 2, MidpointRounding.AwayFromZero);

                lines.Add(new ReceiptLineToInsert(
                    receiptId,
                    "CapRepair",
                    objectType,
                    objectId,
                    Math.Round(area, 2, MidpointRounding.AwayFromZero),
                    ratePerSqm,
                    amount
                ));
            }
        }

        await AddFromAsync("SELECT ApartmentId, Area FROM Apartments WHERE ResidentId = @id;", "Apartment");
        await AddFromAsync("SELECT StorageRoomId, Area FROM StorageRooms WHERE OwnerId = @id;", "StorageRoom");
        await AddFromAsync("SELECT ParkingRoomId, Area FROM ParkingRooms WHERE OwnerId = @id;", "ParkingRoom");

        return lines;
    }

    /// <summary>
    /// Начисление по разнице показаний за месяц: текущий месяц минус предыдущее показание.
    /// Без предыдущего показания строку не создаём (иначе спишем весь накопленный счётчик).
    /// </summary>
    private static async Task<List<ReceiptLineToInsert>> BuildElectricityLinesAsync(
        DbConnection connection,
        DbTransaction tx,
        int residentId,
        int receiptId,
        decimal rubPerKwh,
        DateTime billingMonthFirstDay)
    {
        var lines = new List<ReceiptLineToInsert>();
        if (rubPerKwh <= 0) return lines;

        var meters = new List<(int MeterId, string ObjectType, int ObjectId)>();
        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = @"
SELECT
  m.ElectricMeterId,
  CASE
    WHEN m.ApartmentId IS NOT NULL THEN N'Apartment'
    WHEN m.StorageRoomId IS NOT NULL THEN N'StorageRoom'
    ELSE N'ParkingRoom'
  END,
  COALESCE(m.ApartmentId, m.StorageRoomId, m.ParkingRoomId)
FROM dbo.ElectricMeters m
WHERE
  (m.ApartmentId IS NOT NULL AND EXISTS (
    SELECT 1 FROM dbo.Apartments a WHERE a.ApartmentId = m.ApartmentId AND a.ResidentId = @rid))
  OR (m.StorageRoomId IS NOT NULL AND EXISTS (
    SELECT 1 FROM dbo.StorageRooms s WHERE s.StorageRoomId = m.StorageRoomId AND s.OwnerId = @rid))
  OR (m.ParkingRoomId IS NOT NULL AND EXISTS (
    SELECT 1 FROM dbo.ParkingRooms p WHERE p.ParkingRoomId = m.ParkingRoomId AND p.OwnerId = @rid));";
            cmd.Parameters.Add(DbMini.P(cmd, "@rid", residentId));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                meters.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
            }
        }

        foreach (var (meterId, objectType, objectId) in meters)
        {
            decimal? cur = null;
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
SELECT ReadingValue FROM dbo.ElectricMeterReadings
WHERE ElectricMeterId = @m AND ReadingMonth = @month;";
                cmd.Parameters.Add(DbMini.P(cmd, "@m", meterId));
                cmd.Parameters.Add(DbMini.P(cmd, "@month", billingMonthFirstDay.Date));
                var o = await cmd.ExecuteScalarAsync();
                if (o != null && o != DBNull.Value) cur = Convert.ToDecimal(o);
            }

            if (!cur.HasValue) continue;

            decimal? prev = null;
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
SELECT TOP 1 ReadingValue FROM dbo.ElectricMeterReadings
WHERE ElectricMeterId = @m AND ReadingMonth < @month
ORDER BY ReadingMonth DESC;";
                cmd.Parameters.Add(DbMini.P(cmd, "@m", meterId));
                cmd.Parameters.Add(DbMini.P(cmd, "@month", billingMonthFirstDay.Date));
                var o = await cmd.ExecuteScalarAsync();
                if (o != null && o != DBNull.Value) prev = Convert.ToDecimal(o);
            }

            if (!prev.HasValue) continue;

            var kwh = cur.Value - prev.Value;
            if (kwh < 0) kwh = 0;

            var amount = Math.Round(kwh * rubPerKwh, 2, MidpointRounding.AwayFromZero);
            lines.Add(new ReceiptLineToInsert(
                receiptId,
                "Electricity",
                objectType,
                objectId,
                Math.Round(kwh, 2, MidpointRounding.AwayFromZero),
                rubPerKwh,
                amount
            ));
        }

        return lines;
    }

    private static async Task InsertReceiptLineAsync(DbConnection connection, DbTransaction tx, ReceiptLineToInsert line)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = @"
            INSERT INTO ReceiptLines (ReceiptId, ServiceCode, ObjectType, ObjectId, AreaSqm, RatePerSqm, Amount)
            VALUES (@rid, @code, @otype, @oid, @area, @rate, @amount);";

        command.Parameters.Add(DbMini.P(command, "@rid", line.ReceiptId));
        command.Parameters.Add(DbMini.P(command, "@code", line.ServiceCode));
        command.Parameters.Add(DbMini.P(command, "@otype", line.ObjectType));
        command.Parameters.Add(DbMini.P(command, "@oid", line.ObjectId));
        command.Parameters.Add(DbMini.P(command, "@area", line.AreaSqm));
        command.Parameters.Add(DbMini.P(command, "@rate", line.RatePerSqm));
        command.Parameters.Add(DbMini.P(command, "@amount", line.Amount));

        await command.ExecuteNonQueryAsync();
    }
}

