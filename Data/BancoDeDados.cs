using Microsoft.Data.Sqlite;
using SistemaReservas.Models;

namespace SistemaReservas.Data;

public class BancoDeDados
{
    private readonly SqliteConnection _con;
    public string CaminhoArquivo { get; }

    public BancoDeDados(string? caminho = null)
    {
        CaminhoArquivo = caminho ?? Path.Combine(Directory.GetCurrentDirectory(), "reservas.db");
        _con = new SqliteConnection($"Data Source={CaminhoArquivo}");
        _con.Open();
        CriarEsquema();
    }

    private void CriarEsquema()
    {
        Executar("""
            CREATE TABLE IF NOT EXISTS Cliente (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Telefone TEXT,
                Cpf TEXT,
                Email TEXT,
                QtdVisitas INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS Funcionario (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Telefone TEXT,
                Cpf TEXT,
                Email TEXT,
                Cargo TEXT NOT NULL,
                Turno TEXT NOT NULL,
                Setor TEXT NULL
            );
            CREATE TABLE IF NOT EXISTS Mesa (
                Numero INTEGER PRIMARY KEY,
                Capacidade INTEGER NOT NULL,
                Localizacao TEXT NOT NULL,
                Status TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Reserva (
                Id INTEGER PRIMARY KEY,
                IdCliente INTEGER NOT NULL REFERENCES Cliente(Id),
                IdFuncionario INTEGER REFERENCES Funcionario(Id),
                NumeroMesa INTEGER NOT NULL REFERENCES Mesa(Numero),
                DataHora TEXT NOT NULL,
                NumeroPessoas INTEGER NOT NULL,
                Status TEXT NOT NULL
            );
            """);
    }

    public void Executar(string sql)
    {
        using var cmd = _con.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    /* ---------- identificadores ---------- */
    private int ProximoId(string tabela, string coluna)
    {
        using var cmd = _con.CreateCommand();
        cmd.CommandText = $"SELECT IFNULL(MAX({coluna}), 0) + 1 FROM {tabela};";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
    public int ProximoIdCliente() => ProximoId("Cliente", "Id");
    public int ProximoIdFuncionario() => ProximoId("Funcionario", "Id");
    public int ProximoNumeroMesa() => ProximoId("Mesa", "Numero");
    public int ProximoIdReserva() => ProximoId("Reserva", "Id");

    /* ---------- cliente ---------- */
    public void InserirCliente(Cliente c) => Executar(
        $"""
        INSERT INTO Cliente (Id, Nome, Telefone, Cpf, Email, QtdVisitas)
        VALUES ({c.Id}, {S(c.Nome)}, {S(c.Telefone)}, {S(c.Cpf)}, {S(c.Email)}, {c.QtdVisitas});
        """);

    public void AtualizarCliente(Cliente c) => Executar(
        $"""
        UPDATE Cliente SET Nome = {S(c.Nome)}, Telefone = {S(c.Telefone)},
               Cpf = {S(c.Cpf)}, Email = {S(c.Email)}, QtdVisitas = {c.QtdVisitas}
        WHERE Id = {c.Id};
        """);

    public List<Cliente> CarregarClientes()
    {
        var lista = new List<Cliente>();
        using var cmd = _con.CreateCommand();
        cmd.CommandText = "SELECT Id, Nome, Telefone, Cpf, Email, QtdVisitas FROM Cliente ORDER BY Id;";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var c = new Cliente(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? "" : r.GetString(2),
                                r.IsDBNull(3) ? "" : r.GetString(3), r.IsDBNull(4) ? "" : r.GetString(4));
            c.RestaurarQtdVisitas(r.GetInt32(5));
            lista.Add(c);
        }
        return lista;
    }

    /* ---------- funcionário / gerente ---------- */
    public void InserirFuncionario(Funcionario f) => Executar(
        $"""
        INSERT INTO Funcionario (Id, Nome, Telefone, Cpf, Email, Cargo, Turno, Setor)
        VALUES ({f.Id}, {S(f.Nome)}, {S(f.Telefone)}, {S(f.Cpf)}, {S(f.Email)},
                {S(f.Cargo)}, {S(f.Turno.ToString())}, {S((f as Gerente)?.Setor ?? "")});
        """);

    public void AtualizarFuncionario(Funcionario f) => Executar(
        $"""
        UPDATE Funcionario SET Nome = {S(f.Nome)}, Telefone = {S(f.Telefone)},
               Cpf = {S(f.Cpf)}, Email = {S(f.Email)}, Cargo = {S(f.Cargo)},
               Turno = {S(f.Turno.ToString())}, Setor = {S((f as Gerente)?.Setor ?? "")}
        WHERE Id = {f.Id};
        """);

    public List<Funcionario> CarregarFuncionarios()
    {
        var lista = new List<Funcionario>();
        using var cmd = _con.CreateCommand();
        cmd.CommandText = "SELECT Id, Nome, Telefone, Cpf, Email, Cargo, Turno, Setor FROM Funcionario ORDER BY Id;";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var id = r.GetInt32(0);
            var setor = r.IsDBNull(7) ? null : r.GetString(7);
            var turno = Enum.TryParse<Turno>(r.GetString(6), out var t) ? t : Turno.Manha;
            if (setor is not null)
                lista.Add(new Gerente(id, r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4),
                                      r.GetString(5), turno, setor));
            else
                lista.Add(new Funcionario(id, r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4),
                                          r.GetString(5), turno));
        }
        return lista;
    }

    /* ---------- mesa ---------- */
    public void InserirMesa(Mesa m) => Executar(
        $"""
        INSERT INTO Mesa (Numero, Capacidade, Localizacao, Status)
        VALUES ({m.Numero}, {m.Capacidade}, {S(m.Localizacao)}, {S(m.Status.ToString())});
        """);

    public void AtualizarMesa(Mesa m) => Executar(
        $"""
        UPDATE Mesa SET Capacidade = {m.Capacidade},
               Localizacao = {S(m.Localizacao)}, Status = {S(m.Status.ToString())}
        WHERE Numero = {m.Numero};
        """);

    public List<Mesa> CarregarMesas()
    {
        var lista = new List<Mesa>();
        using var cmd = _con.CreateCommand();
        cmd.CommandText = "SELECT Numero, Capacidade, Localizacao, Status FROM Mesa ORDER BY Numero;";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var m = new Mesa(r.GetInt32(0), r.GetInt32(1), r.GetString(2));
            if (Enum.TryParse<StatusMesa>(r.GetString(3), out var st)) m.Status = st;
            lista.Add(m);
        }
        return lista;
    }

    /* ---------- reserva ---------- */
    public void InserirReserva(Reserva r)
    {
        var func = r.FuncionarioResponsavel is null ? "NULL" : r.FuncionarioResponsavel.Id.ToString();
        Executar(
            $"""
            INSERT INTO Reserva (Id, IdCliente, IdFuncionario, NumeroMesa, DataHora, NumeroPessoas, Status)
            VALUES ({r.IdReserva}, {r.Cliente.Id}, {func}, {r.Mesa.Numero},
                    {S(r.DataHora.ToString("o"))}, {r.NumeroPessoas}, {S(r.Status.ToString())});
            """);
    }

    public void AtualizarReserva(Reserva r)
    {
        var func = r.FuncionarioResponsavel is null ? "NULL" : r.FuncionarioResponsavel.Id.ToString();
        Executar(
            $"""
            UPDATE Reserva SET IdCliente = {r.Cliente.Id},
                   IdFuncionario = {func}, NumeroMesa = {r.Mesa.Numero},
                   DataHora = {S(r.DataHora.ToString("o"))},
                   NumeroPessoas = {r.NumeroPessoas}, Status = {S(r.Status.ToString())}
            WHERE Id = {r.IdReserva};
            """);
    }

    public List<Reserva> CarregarReservas(List<Cliente> clientes, List<Funcionario> funcionarios, List<Mesa> mesas)
    {
        var lista = new List<Reserva>();
        var clientesPorId = clientes.ToDictionary(c => c.Id);
        var funcsPorId = funcionarios.ToDictionary(f => f.Id);
        var mesasPorNum = mesas.ToDictionary(m => m.Numero);
        using var cmd = _con.CreateCommand();
        cmd.CommandText = "SELECT Id, IdCliente, IdFuncionario, NumeroMesa, DataHora, NumeroPessoas, Status FROM Reserva ORDER BY DataHora;";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            if (!clientesPorId.TryGetValue(r.GetInt32(1), out var cli)) continue;
            if (!mesasPorNum.TryGetValue(r.GetInt32(3), out var mesa)) continue;
            Funcionario? resp = r.IsDBNull(2) || !funcsPorId.TryGetValue(r.GetInt32(2), out var f) ? null : f;
            var dt = DateTime.Parse(r.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind);
            var reserva = new Reserva(r.GetInt32(0), cli, mesa, dt, r.GetInt32(5));
            if (Enum.TryParse<StatusReserva>(r.GetString(6), out var st))
                reserva.Restaurar(st, resp);
            lista.Add(reserva);
        }
        foreach (var mesa in mesas)
        {
            mesa.Status = lista.Any(r => r.Mesa.Numero == mesa.Numero && r.Status == StatusReserva.Confirmada)
                ? StatusMesa.Reservada
                : mesa.Status;
        }
        return lista;
    }

    private static string S(string valor) => "'" + (valor ?? "").Replace("'", "''") + "'";
}