namespace SistemaReservas.Models;

public class Gerente : Funcionario
{
    public string Setor { get; set; }

    public Gerente(int id, string nome, string telefone, string cpf, string email, string cargo, Turno turno, string setor)
        : base(id, nome, telefone, cpf, email, cargo, turno)
    {
        Setor = setor;
    }

    public List<Reserva> GerarRelatorio(List<Reserva> reservas, DateTime data)
        => reservas
            .Where(r => r.DataHora.Date == data.Date && r.Status != StatusReserva.Cancelada)
            .OrderBy(r => r.DataHora)
            .ToList();

    public override string Tipo() => "Gerente";

    public override string ExibirInformacoes()
        => $"{base.ExibirInformacoes()} | Setor: {Setor}";
}