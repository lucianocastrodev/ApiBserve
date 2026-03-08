using Dapper;
using Npgsql;
using ApiBserve.Models;
using System.Data;

namespace ApiBserve.Data;

public class ClienteRepository
{
    private readonly string _connectionString;

    public ClienteRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    private IDbConnection Connection()
        => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<Cliente>> GetAll()
    {
        using var db = Connection();
        return await db.QueryAsync<Cliente>(
            "SELECT id, nome, email FROM clientes ORDER BY id DESC");
    }

    public async Task<Cliente?> GetById(int id)
    {
        using var db = Connection();
        return await db.QueryFirstOrDefaultAsync<Cliente>(
            "SELECT id, nome, email FROM clientes WHERE id = @Id",
            new { Id = id });
    }

    public async Task<int> Create(Cliente cliente)
    {
        using var db = Connection();

        var sql = @"INSERT INTO clientes (nome, email)
                    VALUES (@Nome, @Email)
                    RETURNING id;";

        return await db.ExecuteScalarAsync<int>(sql, cliente);
    }

    public async Task<bool> Update(Cliente cliente)
    {
        using var db = Connection();

        var sql = @"UPDATE clientes
                    SET nome = @Nome,
                        email = @Email
                    WHERE id = @Id;";

        var rows = await db.ExecuteAsync(sql, cliente);
        return rows > 0;
    }

    public async Task<bool> Delete(int id)
    {
        using var db = Connection();
        var rows = await db.ExecuteAsync(
            "DELETE FROM clientes WHERE id = @Id",
            new { Id = id });

        return rows > 0;
    }
}