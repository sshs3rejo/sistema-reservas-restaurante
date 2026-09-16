namespace SistemaReservas.Models;

public class Reserva
{
    public int IdReserva { get; }
    public DateTime DataHora { get; set; }
    public int NumeroPessoas { get; }
    public StatusReserva Status { get; private set; } = StatusReserva.Pendente;
    public Cliente Cliente { get; }
    public Mesa Mesa { get; }
    public Funcionario? FuncionarioResponsavel { get; private set; }

    public Reserva(int idReserva, Cliente cliente, Mesa mesa, DateTime dataHora, int numeroPessoas)
    {
        IdReserva = idReserva;
        Cliente = cliente;
        Mesa = mesa;
        DataHora = dataHora;
        NumeroPessoas = numeroPessoas;
    }

    public void Confirmar(Funcionario funcionario)
    {
        Status = StatusReserva.Confirmada;
        FuncionarioResponsavel = funcionario;
        Mesa.Status = StatusMesa.Reservada;
    }

    public void Cancelar()
    {
        Status = StatusReserva.Cancelada;
        Mesa.Status = StatusMesa.Disponivel;
    }

    public void Concluir()
    {
        Status = StatusReserva.Concluida;
        Mesa.Status = StatusMesa.Disponivel;
        Cliente.RegistrarVisita();
    }

    internal void Restaurar(StatusReserva status, Funcionario? responsavel)
    {
        Status = status;
        FuncionarioResponsavel = responsavel;
    }

    public override string ToString()
        => $"Reserva #{IdReserva}: {DataHora:dd/MM/yyyy HH:mm} | {NumeroPessoas} pessoas | Mesa {Mesa.Numero} | {Cliente.Nome} | {Status}";
}