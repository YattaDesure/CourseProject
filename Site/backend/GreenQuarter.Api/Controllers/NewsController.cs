using System.Data.Common;
using GreenQuarter.Infrastructure.Data;
using GreenQuarter.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenQuarter.Api.Controllers;

[ApiController]
[Route("api/news")]
[Authorize]
public class NewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatest([FromQuery] int limit = 5)
    {
        if (limit <= 0) limit = 5;
        if (limit > 20) limit = 20;

        // Тут проще открыть один раз и дальше использовать.
        var connection = await DbMini.OpenAsync(_context);

        var items = new List<object>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT TOP (@limit)
                    NewsPostId,
                    Title,
                    Body,
                    CreatedAt,
                    IsPinned
                FROM NewsPosts
                WHERE IsPublished = 1
                ORDER BY IsPinned DESC, CreatedAt DESC;";

            command.Parameters.Add(DbMini.P(command, "@limit", limit));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new
                {
                    Id = reader.GetInt32(0),
                    Title = reader.IsDBNull(1) ? "" : (reader.GetValue(1)?.ToString() ?? ""),
                    Body = reader.IsDBNull(2) ? "" : (reader.GetValue(2)?.ToString() ?? ""),
                    CreatedAt = reader.GetDateTime(3),
                    IsPinned = !reader.IsDBNull(4) && reader.GetBoolean(4)
                });
            }
        }

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateNewsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(new { message = "Body is required." });
        }

        var connection = await DbMini.OpenAsync(_context);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO NewsPosts (Title, Body, IsPinned, IsPublished)
                VALUES (@title, @body, @pinned, 1);
                SELECT SCOPE_IDENTITY();";

            command.Parameters.Add(DbMini.P(command, "@title", string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim()));
            command.Parameters.Add(DbMini.P(command, "@body", request.Body.Trim()));
            command.Parameters.Add(DbMini.P(command, "@pinned", request.IsPinned));

            var insertedId = await command.ExecuteScalarAsync();
            return Ok(new { Id = Convert.ToInt32(insertedId) });
        }
        catch (DbException ex)
        {
            return StatusCode(500, new { message = "Database error", detail = ex.Message });
        }
    }
}

public sealed class CreateNewsRequest
{
    public string? Title { get; set; }
    public string Body { get; set; } = "";
    public bool IsPinned { get; set; }
}

