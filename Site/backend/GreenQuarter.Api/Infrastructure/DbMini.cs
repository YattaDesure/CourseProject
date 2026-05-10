using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace GreenQuarter.Api.Infrastructure;

// Маленькая "шпаргалка" для работы с raw SQL.
// Чтобы в контроллерах не копировать 20 раз одно и то же (open connection + параметры).
public static class DbMini
{
    public static async Task<DbConnection> OpenAsync(DbContext db)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }

    public static DbParameter P(DbCommand command, string name, object? value)
    {
        var p = command.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        return p;
    }
}

