namespace SistemaReservas.Models;

public class Funcionario : Pessoa
{
    public string Cargo { get; set; }
    public Turno Turno { get; set; }

    public Funcionario(int id, string nome, string telefone, string cpf, string email, string cargo, Turno turno)
        : base(id, nome, telefone, cpf, email)
    {
        Cargo = cargo;
        Turno = turno;
    }

    public void ConfirmarReserva(Reserva reserva) => reserva.Confirmar(this);

    public void ReagendarReserva(Reserva reserva, DateTime novaDataHora)
    {
        reserva.DataHora = novaDataHora;
        reserva.Confirmar(this);
    }

    public override string Tipo() => "Funcionario";

    public override string ExibirInformacoes()
        => $"{base.ExibirInformacoes()} | Cargo: {Cargo} | Turno: {Turno}";
}