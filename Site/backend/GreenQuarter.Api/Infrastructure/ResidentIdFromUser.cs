using System.Security.Claims;
using GreenQuarter.Infrastructure.Data;

namespace GreenQuarter.Api.Infrastructure;

/// <summary>
/// JWT для Residents содержит числовой ResidentId в NameIdentifier.
/// Для AspNetUsers там GUID — тогда ищем ResidentId по Email в таблице Residents.
/// </summary>
public static class ResidentIdFromUser
{
    public static async Task<int?> ResolveAsync(ClaimsPrincipal user, ApplicationDbContext context)
    {
        var ridClaim = user.FindFirst("ResidentId")?.Value;
        if (!string.IsNullOrEmpty(ridClaim) && int.TryParse(ridClaim, out var rid)) return rid;

        var nameId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(nameId) && int.TryParse(nameId, out var rid2)) return rid2;

        var email = user.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email)) return null;

        var connection = await DbMini.OpenAsync(context);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT TOP 1 ResidentId FROM Residents WHERE Email = @e";
        cmd.Parameters.Add(DbMini.P(cmd, "@e", email.Trim()));
        var o = await cmd.ExecuteScalarAsync();
        if (o is null || o == DBNull.Value) return null;
        return Convert.ToInt32(o);
    }
}
