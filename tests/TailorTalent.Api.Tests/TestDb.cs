using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TailorTalent.Api.Data;

namespace TailorTalent.Api.Tests;

/// <summary>
/// Creates an isolated SQLite in-memory database per instance.
/// The connection must stay open for the lifetime of the database.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public TailorTalentDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TailorTalentDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new TailorTalentDbContext(options);
        Context.Database.EnsureCreated();
    }

    public TailorTalentDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<TailorTalentDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new TailorTalentDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
