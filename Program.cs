using SistemaReservas.Data;
using SistemaReservas.Models;

var repo = new BancoDeDados();

var restaurante = new Restaurante("Bella Città", "Av. Principal, 100 - Caxias/MA");
var clientes = repo.CarregarClientes();
var funcionarios = repo.CarregarFuncionarios();

var mesas = repo.CarregarMesas();
restaurante.Mesas.AddRange(mesas);

var reservas = repo.CarregarReservas(clientes, funcionarios, restaurante.Mesas);
restaurante.Reservas.AddRange(reservas);
restaurante.AjustarContadores(
    mesas.Count > 0 ? mesas.Max(m => m.Numero) + 1 : 1,
    reservas.Count > 0 ? reservas.Max(r => r.IdReserva) + 1 : 1);

if (restaurante.Mesas.Count == 0 && clientes.Count == 0 && funcionarios.Count == 0)
{
    Console.Write("Banco vazio. Carregar dados de exemplo? (s/n): ");
    if ((Console.ReadLine() ?? "s").Trim().ToLowerInvariant() == "s")
        CarregarDadosExemplo(repo, restaurante, clientes, funcionarios);
}

int opcao;
do
{
    try { Console.Clear(); } catch { }
    Console.WriteLine($"=== {restaurante.Nome} - Sistema de Reservas ===");
    Console.WriteLine($"Endereço: {restaurante.Endereco}  |  Banco: {Path.GetFileName(repo.CaminhoArquivo)}");
    Console.WriteLine($"Clientes: {clientes.Count}  |  Funcionários: {funcionarios.Count}  |  Mesas: {restaurante.Mesas.Count}  |  Reservas: {restaurante.Reservas.Count}\n");
    Console.WriteLine(" 1. Cadastrar cliente");
    Console.WriteLine(" 2. Cadastrar funcionário");
    Console.WriteLine(" 3. Cadastrar gerente");
    Console.WriteLine(" 4. Cadastrar mesa");
    Console.WriteLine(" 5. Consultar disponibilidade");
    Console.WriteLine(" 6. Realizar reserva");
    Console.WriteLine(" 7. Confirmar reserva");
    Console.WriteLine(" 8. Cancelar reserva");
    Console.WriteLine(" 9. Concluir reserva");
    Console.WriteLine("10. Reagendar reserva");
    Console.WriteLine("11. Agenda do dia");
    Console.WriteLine("12. Relatório do dia (gerente)");
    Console.WriteLine("13. Listar registros");
    Console.WriteLine(" 0. Sair");
    opcao = LerInt("\nOpção: ", 0, 13);

    switch (opcao)
    {
        case 1: CadastrarCliente(repo, clientes); break;
        case 2: CadastrarFuncionario(repo, funcionarios, gerente: false); break;
        case 3: CadastrarFuncionario(repo, funcionarios, gerente: true); break;
        case 4: CadastrarMesa(repo, restaurante); break;
        case 5: ConsultarDisponibilidade(restaurante); break;
        case 6: RealizarReserva(repo, restaurante, clientes); break;
        case 7: ConfirmarReserva(repo, restaurante, funcionarios, reservas); break;
        case 8: CancelarReserva(repo, restaurante, reservas); break;
        case 9: ConcluirReserva(repo, restaurante, clientes, reservas); break;
        case 10: ReagendarReserva(repo, restaurante, funcionarios, reservas); break;
        case 11: AgendaDoDia(restaurante); break;
        case 12: RelatorioDoDia(funcionarios, restaurante); break;
        case 13: Listar(clientes, funcionarios, restaurante); break;
    }
    if (opcao != 0)
    {
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }
} while (opcao != 0);

Console.WriteLine("Banco salvo em: " + repo.CaminhoArquivo);

/* ================= rotinas do menu ================= */

void CadastrarCliente(BancoDeDados db, List<Cliente> clientes)
{
    var nome = Ler("Nome: ");
    var telefone = Ler("Telefone: ");
    var cpf = Ler("CPF: ");
    var email = Ler("E-mail: ");
    var cliente = new Cliente(db.ProximoIdCliente(), nome, telefone, cpf, email);
    db.InserirCliente(cliente);
    clientes.Add(cliente);
    Console.WriteLine($"Cliente cadastrado com sucesso! [{cliente.ExibirInformacoes()}]");
}

void CadastrarFuncionario(BancoDeDados db, List<Funcionario> funcionarios, bool gerente)
{
    var nome = Ler("Nome: ");
    var telefone = Ler("Telefone: ");
    var cpf = Ler("CPF: ");
    var email = Ler("E-mail: ");
    var cargo = Ler("Cargo: ");
    var turno = LerTurno();
    var id = db.ProximoIdFuncionario();
    Funcionario f;
    if (gerente)
    {
        var setor = Ler("Setor: ");
        f = new Gerente(id, nome, telefone, cpf, email, cargo, turno, setor);
    }
    else
    {
        f = new Funcionario(id, nome, telefone, cpf, email, cargo, turno);
    }
    db.InserirFuncionario(f);
    funcionarios.Add(f);
    Console.WriteLine($"{(gerente ? "Gerente" : "Funcionário")} cadastrado: [{f.ExibirInformacoes()}]");
}

void CadastrarMesa(BancoDeDados db, Restaurante restaurante)
{
    var capacidade = LerInt("Capacidade: ", 1, 50);
    var localizacao = Ler("Localização: ");
    var mesa = restaurante.AdicionarMesa(capacidade, localizacao);
    db.InserirMesa(mesa);
    Console.WriteLine($"Mesa cadastrada: {mesa}");
}

void ConsultarDisponibilidade(Restaurante restaurante)
{
    if (restaurante.Mesas.Count == 0)
    {
        Console.WriteLine("Nenhuma mesa cadastrada ainda.");
        return;
    }
    var data = LerDataHora("Data e hora (dd/MM/yyyy HH:mm): ");
    var pessoas = LerInt("Número de pessoas: ", 1, 50);
    var disponiveis = restaurante.ConsultarDisponibilidade(data, pessoas);
    Console.WriteLine(disponiveis.Count == 0
        ? "Nenhuma mesa disponível para esse horário."
        : $"{disponiveis.Count} mesa(s) disponível(is):");
    foreach (var m in disponiveis) Console.WriteLine($"  {m}");
}

void RealizarReserva(BancoDeDados db, Restaurante restaurante, List<Cliente> clientes)
{
    if (clientes.Count == 0) { Console.WriteLine("Cadastre um cliente primeiro."); return; }
    if (restaurante.Mesas.Count == 0) { Console.WriteLine("Cadastre uma mesa primeiro."); return; }
    var cliente = Escolher(clientes, c => $"#{c.Id} - {c.Nome}", "Cliente");
    var data = LerDataHora("Data e hora (dd/MM/yyyy HH:mm): ");
    var pessoas = LerInt("Número de pessoas: ", 1, 50);

    var disponiveis = restaurante.ConsultarDisponibilidade(data, pessoas);
    if (disponiveis.Count == 0)
    {
        Console.WriteLine("Nenhuma mesa disponível nesse horário.");
        return;
    }
    Console.WriteLine("Mesas disponíveis:");
    foreach (var m in disponiveis) Console.WriteLine($"  {m}");
    var mesa = Escolher(disponiveis, m => $"Mesa {m.Numero} ({m.Capacidade} lugares)", "Mesa");

    var reserva = restaurante.RealizarReserva(cliente, mesa, data, pessoas);
    db.InserirReserva(reserva);
    db.AtualizarMesa(mesa);
    Console.WriteLine($"Reserva realizada: {reserva}");
}

void ConfirmarReserva(BancoDeDados db, Restaurante restaurante, List<Funcionario> funcionarios, List<Reserva> reservas)
{
    if (funcionarios.Count == 0) { Console.WriteLine("Cadastre um funcionário primeiro."); return; }
    var candidatas = reservas.Where(r => r.Status == StatusReserva.Pendente).ToList();
    if (candidatas.Count == 0) { Console.WriteLine("Não há reservas pendentes."); return; }
    var reserva = Escolher(candidatas, r => r.ToString(), "Reserva");
    var funcionario = Escolher(funcionarios, f => $"#{f.Id} - {f.Nome}", "Funcionário");
    funcionario.ConfirmarReserva(reserva);
    db.AtualizarReserva(reserva);
    Console.WriteLine($"Reserva confirmada por {funcionario.Nome}: {reserva}");
}

void CancelarReserva(BancoDeDados db, Restaurante restaurante, List<Reserva> reservas)
{
    var candidatas = reservas.Where(r => r.Status is StatusReserva.Pendente or StatusReserva.Confirmada).ToList();
    if (candidatas.Count == 0) { Console.WriteLine("Não há reservas para cancelar."); return; }
    var reserva = Escolher(candidatas, r => r.ToString(), "Reserva");
    reserva.Cancelar();
    db.AtualizarReserva(reserva);
    db.AtualizarMesa(reserva.Mesa);
    Console.WriteLine($"Reserva cancelada: {reserva}");
}

void ConcluirReserva(BancoDeDados db, Restaurante restaurante, List<Cliente> clientes, List<Reserva> reservas)
{
    var candidatas = reservas.Where(r => r.Status == StatusReserva.Confirmada).ToList();
    if (candidatas.Count == 0) { Console.WriteLine("Não há reservas confirmadas."); return; }
    var reserva = Escolher(candidatas, r => r.ToString(), "Reserva");
    reserva.Concluir();
    db.AtualizarReserva(reserva);
    db.AtualizarMesa(reserva.Mesa);
    db.AtualizarCliente(reserva.Cliente);
    Console.WriteLine($"Reserva concluída (visita registrada): {reserva}");
}

void ReagendarReserva(BancoDeDados db, Restaurante restaurante, List<Funcionario> funcionarios, List<Reserva> reservas)
{
    if (funcionarios.Count == 0) { Console.WriteLine("Cadastre um funcionário primeiro."); return; }
    var candidatas = reservas.Where(r => r.Status is StatusReserva.Pendente or StatusReserva.Confirmada).ToList();
    if (candidatas.Count == 0) { Console.WriteLine("Não há reservas para reagendar."); return; }
    var reserva = Escolher(candidatas, r => r.ToString(), "Reserva");
    var novaData = LerDataHora("Nova data e hora (dd/MM/yyyy HH:mm): ");
    var funcionario = Escolher(funcionarios, f => $"#{f.Id} - {f.Nome}", "Funcionário");
    funcionario.ReagendarReserva(reserva, novaData);
    db.AtualizarReserva(reserva);
    db.AtualizarMesa(reserva.Mesa);
    Console.WriteLine($"Reserva reagendada: {reserva}");
}

void AgendaDoDia(Restaurante restaurante)
{
    var data = LerDataHora("Data (dd/MM/yyyy): ");
    var agenda = restaurante.AgendaDoDia(data);
    Console.WriteLine(agenda.Count == 0 ? "Nenhuma reserva nesse dia." : $"Agenda de {data:dd/MM/yyyy}:");
    foreach (var r in agenda) Console.WriteLine($"  {r}");
}

void RelatorioDoDia(List<Funcionario> funcionarios, Restaurante restaurante)
{
    var gerentes = funcionarios.Where(f => f is Gerente).Cast<Gerente>().ToList();
    if (gerentes.Count == 0) { Console.WriteLine("Cadastre um gerente primeiro."); return; }
    var gerente = Escolher(gerentes, g => $"#{g.Id} - {g.Nome}", "Gerente");
    var data = LerDataHora("Data (dd/MM/yyyy): ");
    Console.WriteLine($"Relatório de {data:dd/MM/yyyy} (gerado por {gerente.Nome}):");
    foreach (var r in gerente.GerarRelatorio(restaurante.Reservas, data.Date))
        Console.WriteLine($"  {r}");
}

void Listar(List<Cliente> clientes, List<Funcionario> funcionarios, Restaurante restaurante)
{
    Console.WriteLine($"\n--- CLIENTES ({clientes.Count}) ---");
    foreach (var c in clientes) Console.WriteLine($"  {c.ExibirInformacoes()}");
    Console.WriteLine($"\n--- FUNCIONÁRIOS ({funcionarios.Count}) ---");
    foreach (var f in funcionarios) Console.WriteLine($"  {f.Tipo()}: {f.ExibirInformacoes()}");
    Console.WriteLine($"\n--- MESAS ({restaurante.Mesas.Count}) ---");
    foreach (var m in restaurante.Mesas) Console.WriteLine($"  {m}");
    Console.WriteLine($"\n--- RESERVAS ({restaurante.Reservas.Count}) ---");
    foreach (var r in restaurante.Reservas.OrderBy(r => r.DataHora)) Console.WriteLine($"  {r}");
}

/* ================= utilitários de entrada ================= */

string Ler(string msg)
{
    Console.Write(msg);
    return (Console.ReadLine() ?? "").Trim();
}

int LerInt(string msg, int min, int max)
{
    while (true)
    {
        Console.Write(msg);
        if (int.TryParse(Console.ReadLine()?.Trim(), out var v) && v >= min && v <= max)
            return v;
        Console.WriteLine($"Valor inválido. Informe um número entre {min} e {max}.");
    }
}

DateTime LerDataHora(string msg)
{
    while (true)
    {
        Console.Write(msg);
        var partes = (Console.ReadLine() ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (DateTime.TryParseExact(partes[0], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var data))
        {
            TimeSpan tempo = new(12, 0, 0);
            if (partes.Length >= 2)
            {
                if (DateTime.TryParseExact(partes[1], "HH:mm", System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var hora))
                    tempo = hora.TimeOfDay;
                else
                {
                    Console.WriteLine("Hora inválida. Use HH:mm.");
                    continue;
                }
            }
            var dataHora = data.Date.Add(tempo);
            if (dataHora >= DateTime.Now.AddMinutes(-5)) return dataHora;
            Console.WriteLine("A data/hora deve ser futura.");
            continue;
        }
        Console.WriteLine("Formato inválido. Use dd/MM/yyyy HH:mm (ex.: 20/10/2026 20:00).");
    }
}

Turno LerTurno()
{
    Console.Write("Turno (1-Manhã, 2-Tarde, 3-Noite): ");
    return (LerInt("", 1, 3)) switch
    {
        1 => Turno.Manha,
        2 => Turno.Tarde,
        _ => Turno.Noite,
    };
}

T Escolher<T>(List<T> lista, Func<T, string> descricao, string rotulo)
{
    Console.WriteLine($"Escolha um(a) {rotulo}:");
    for (int i = 0; i < lista.Count; i++)
        Console.WriteLine($"  {i + 1}. {descricao(lista[i])}");
    var idx = LerInt($"Número ({rotulo}): ", 1, lista.Count);
    return lista[idx - 1];
}

/* ================= dados de exemplo ================= */

void CarregarDadosExemplo(BancoDeDados db, Restaurante restaurante, List<Cliente> clientes, List<Funcionario> funcionarios)
{
    var g = new Gerente(db.ProximoIdFuncionario(), "Maria Rocha", "(99) 99999-0001", "111.222.333-44",
                        "maria@email.com", "Gestão", Turno.Tarde, "Operação");
    db.InserirFuncionario(g);
    funcionarios.Add(g);

    var f = new Funcionario(db.ProximoIdFuncionario(), "Carlos Lima", "(99) 99999-0002", "222.333.444-55",
                            "carlos@email.com", "Atendente", Turno.Noite);
    db.InserirFuncionario(f);
    funcionarios.Add(f);

    var c1 = new Cliente(db.ProximoIdCliente(), "Ana Souza", "(99) 99999-0003", "333.444.555-66", "ana@email.com");
    db.InserirCliente(c1);
    clientes.Add(c1);

    var c2 = new Cliente(db.ProximoIdCliente(), "Bruno Dias", "(99) 99999-0004", "444.555.666-77", "bruno@email.com");
    db.InserirCliente(c2);
    clientes.Add(c2);

    restaurante.AdicionarMesa(2, "Varanda");
    restaurante.AdicionarMesa(4, "Salão 1");
    restaurante.AdicionarMesa(4, "Salão 2");
    restaurante.AdicionarMesa(6, "Salão 1");
    foreach (var m in restaurante.Mesas) db.InserirMesa(m);

    var dia = DateTime.Now.Date.AddDays(1).AddHours(20);
    var m4 = restaurante.Mesas.First(m => m.Capacidade >= 4);
    var m6 = restaurante.Mesas.First(m => m.Capacidade >= 6);
    var r1 = restaurante.RealizarReserva(c1, m4, dia, 4);
    db.InserirReserva(r1);
    db.AtualizarMesa(m4);
    var r2 = restaurante.RealizarReserva(c2, m6, dia.AddHours(1), 6);
    db.InserirReserva(r2);
    db.AtualizarMesa(m6);

    Console.WriteLine("Dados de exemplo carregados e salvos no banco.");
}