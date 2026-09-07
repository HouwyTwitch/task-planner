using Microsoft.Data.Sqlite;
using Planner.Core.Models;
using Planner.Data.Database;

namespace Planner.Data.Repositories;

public sealed class UserRepository
{
    private readonly DatabaseExecutor _db;
    public UserRepository(DatabaseExecutor db) => _db = db;

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            await using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, DisplayName, ParentId, Role FROM Users WHERE Username=$u COLLATE NOCASE LIMIT 1";
            cmd.Parameters.AddWithValue("$u", username);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            return await r.ReadAsync(ct) ? ReadUser(r) : null;
        }, ct);

    public Task<User?> GetByIdAsync(long id, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            await using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, DisplayName, ParentId, Role FROM Users WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            return await r.ReadAsync(ct) ? ReadUser(r) : null;
        }, ct);

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            var result = new List<User>();
            await using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, DisplayName, ParentId, Role FROM Users ORDER BY DisplayName";
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) result.Add(ReadUser(r));
            return (IReadOnlyList<User>)result;
        }, ct);

    public Task<IReadOnlyList<User>> GetSelfAndSubordinatesAsync(long currentUserId, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            var result = new List<User>();
            await using var cmd = c.CreateCommand();
            cmd.CommandText = """
WITH RECURSIVE Accessible AS (
    SELECT Id, Username, DisplayName, ParentId, Role FROM Users WHERE Id=$id
    UNION ALL
    SELECT u.Id, u.Username, u.DisplayName, u.ParentId, u.Role
    FROM Users u JOIN Accessible a ON u.ParentId = a.Id
)
SELECT Id, Username, DisplayName, ParentId, Role FROM Accessible ORDER BY DisplayName;
""";
            cmd.Parameters.AddWithValue("$id", currentUserId);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) result.Add(ReadUser(r));
            return (IReadOnlyList<User>)result;
        }, ct);

    public Task<bool> IsSubordinateAsync(long currentUserId, long targetId, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            await using var cmd = c.CreateCommand();
            cmd.CommandText = """
WITH RECURSIVE Subordinates AS (
    SELECT Id, ParentId FROM Users WHERE ParentId=$current
    UNION ALL
    SELECT u.Id, u.ParentId FROM Users u JOIN Subordinates s ON u.ParentId=s.Id
)
SELECT EXISTS(SELECT 1 FROM Subordinates WHERE Id=$target);
""";
            cmd.Parameters.AddWithValue("$current", currentUserId);
            cmd.Parameters.AddWithValue("$target", targetId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) == 1;
        }, ct);

    public Task<User> BootstrapFirstUserAsync(string username, string displayName, CancellationToken ct = default) =>
        _db.WithTransactionAsync(async (c, tx) =>
        {
            await using var count = c.CreateCommand();
            count.Transaction = tx;
            count.CommandText = "SELECT COUNT(*) FROM Users";
            var n = Convert.ToInt64(await count.ExecuteScalarAsync(ct));
            if (n != 0) throw new InvalidOperationException("Начальный администратор уже создан.");

            await using var insert = c.CreateCommand();
            insert.Transaction = tx;
            insert.CommandText = "INSERT INTO Users(Username,DisplayName,ParentId,Role) VALUES($u,$d,NULL,'Admin'); SELECT last_insert_rowid();";
            insert.Parameters.AddWithValue("$u", username);
            insert.Parameters.AddWithValue("$d", displayName);
            var id = Convert.ToInt64(await insert.ExecuteScalarAsync(ct));
            return new User { Id = id, Username = username, DisplayName = displayName, Role = UserRoles.Admin };
        }, ct);

    public Task<long> CreateAsync(User user, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            await using var cmd = c.CreateCommand();
            cmd.CommandText = "INSERT INTO Users(Username,DisplayName,ParentId,Role) VALUES($u,$d,$p,$r); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$u", user.Username.Trim());
            cmd.Parameters.AddWithValue("$d", user.DisplayName.Trim());
            cmd.Parameters.AddWithValue("$p", (object?)user.ParentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$r", user.Role);
            return Convert.ToInt64(await cmd.ExecuteScalarAsync(ct));
        }, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default) =>
        _db.WithTransactionAsync(async (c,tx) =>
        {
            if(user.ParentId is long parentId)
            {
                await using var cycle=c.CreateCommand(); cycle.Transaction=tx;
                cycle.CommandText="""
WITH RECURSIVE Descendants AS (
    SELECT Id FROM Users WHERE ParentId=$id
    UNION ALL
    SELECT u.Id FROM Users u JOIN Descendants d ON u.ParentId=d.Id
)
SELECT EXISTS(SELECT 1 FROM Descendants WHERE Id=$parent);
""";
                cycle.Parameters.AddWithValue("$id",user.Id); cycle.Parameters.AddWithValue("$parent",parentId);
                if(Convert.ToInt32(await cycle.ExecuteScalarAsync(ct))==1) throw new ArgumentException("Изменение создаст цикл в иерархии пользователей.");
            }
            await using var cmd = c.CreateCommand(); cmd.Transaction=tx;
            cmd.CommandText = "UPDATE Users SET Username=$u,DisplayName=$d,ParentId=$p,Role=$r WHERE Id=$id";
            cmd.Parameters.AddWithValue("$u", user.Username.Trim());
            cmd.Parameters.AddWithValue("$d", user.DisplayName.Trim());
            cmd.Parameters.AddWithValue("$p", (object?)user.ParentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$r", user.Role);
            cmd.Parameters.AddWithValue("$id", user.Id);
            await cmd.ExecuteNonQueryAsync(ct);
            return true;
        }, ct);

    private static User ReadUser(SqliteDataReader r) => new()
    {
        Id = r.GetInt64(0), Username = r.GetString(1), DisplayName = r.GetString(2),
        ParentId = r.IsDBNull(3) ? null : r.GetInt64(3), Role = r.GetString(4)
    };
}
