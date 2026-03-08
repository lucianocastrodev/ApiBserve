using ApiBserve.Models;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace ApiBserve.Data;

public class UsuarioRepository
{
    private readonly IConfiguration _config;

    public UsuarioRepository(IConfiguration config)
    {
        _config = config;
    }

    private string ConnectionString =>
        _config.GetConnectionString("DefaultConnection")!;

    // Obter usuário pelo email
    public async Task<Usuario?> ObterPorEmail(string email)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, nome, email, senha_hash, role, ativo FROM usuarios WHERE email = @email",
            conn);

        cmd.Parameters.AddWithValue("email", email);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                Id = reader.GetGuid(0),          // UUID
                Nome = reader.GetString(1),
                Email = reader.GetString(2),
                SenhaHash = reader.GetString(3),
                Role = reader.GetString(4),
                Ativo = reader.GetBoolean(5)
            };
        }

        return null;
    }

    // Criar novo usuário
    public async Task Criar(Usuario usuario)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        // Gera UUID se não existir
        if (usuario.Id == Guid.Empty)
            usuario.Id = Guid.NewGuid();

        // Gera hash da senha (só se ainda não tiver)
        if (!string.IsNullOrEmpty(usuario.SenhaHash))
        {
            var hasher = new PasswordHasher<string>();
            usuario.SenhaHash = hasher.HashPassword(usuario.Email, usuario.SenhaHash);
        }

        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO usuarios (id, nome, email, senha_hash, role, ativo)
          VALUES (@id, @nome, @email, @senha, @role, @ativo)",
            conn);

        cmd.Parameters.AddWithValue("id", usuario.Id);
        cmd.Parameters.AddWithValue("nome", usuario.Nome);
        cmd.Parameters.AddWithValue("email", usuario.Email);
        cmd.Parameters.AddWithValue("senha", usuario.SenhaHash);
        cmd.Parameters.AddWithValue("role", usuario.Role);
        cmd.Parameters.AddWithValue("ativo", usuario.Ativo);

        await cmd.ExecuteNonQueryAsync();
    }

    // Listar todos os usuários
    public async Task<List<Usuario>> ObterTodos()
    {
        var lista = new List<Usuario>();
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, nome, email, senha_hash, role, ativo FROM usuarios ORDER BY nome",
            conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new Usuario
            {
                Id = reader.GetGuid(0),
                Nome = reader.GetString(1),
                Email = reader.GetString(2),
                SenhaHash = reader.GetString(3),
                Role = reader.GetString(4),
                Ativo = reader.GetBoolean(5)
            });
        }

        return lista;
    }

    // Atualizar usuário
    public async Task<bool> Atualizar(Usuario usuario)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"UPDATE usuarios
              SET nome = @nome,
                  email = @email,
                  senha_hash = @senha,
                  role = @role,
                  ativo = @ativo
              WHERE id = @id",
            conn);

        cmd.Parameters.AddWithValue("id", usuario.Id);
        cmd.Parameters.AddWithValue("nome", usuario.Nome);
        cmd.Parameters.AddWithValue("email", usuario.Email);
        cmd.Parameters.AddWithValue("senha", usuario.SenhaHash);
        cmd.Parameters.AddWithValue("role", usuario.Role);
        cmd.Parameters.AddWithValue("ativo", usuario.Ativo);

        var linhasAfetadas = await cmd.ExecuteNonQueryAsync();
        return linhasAfetadas > 0;
    }

    // Deletar usuário
    public async Task<bool> Deletar(Guid id)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("DELETE FROM usuarios WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        var linhasAfetadas = await cmd.ExecuteNonQueryAsync();
        return linhasAfetadas > 0;
    }
}