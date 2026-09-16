namespace SistemaReservas.Models;

public class Restaurante
{
    public string Nome { get; set; }
    public string Endereco { get; set; }
    public List<Mesa> Mesas { get; } = new();
    public List<Reserva> Reservas { get; } = new();

    private int _proximoNumeroMesa = 1;
    private int _proximoIdReserva = 1;

    public Restaurante(string nome, string endereco)
    {
        Nome = nome;
        Endereco = endereco;
    }

    public Mesa AdicionarMesa(int capacidade, string localizacao)
    {
        var mesa = new Mesa(_proximoNumeroMesa++, capacidade, localizacao);
        Mesas.Add(mesa);
        return mesa;
    }

    public List<Mesa> ConsultarDisponibilidade(DateTime dataHora, int pessoas)
        => Mesas
            .Where(m => m.Capacidade >= pessoas)
            .Where(m => m.EstaDisponivel())
            .Where(m => !Reservas.Any(r =>
                r.Mesa.Numero == m.Numero
                && r.Status == StatusReserva.Confirmada
                && r.DataHora == dataHora))
            .ToList();

    public Reserva RealizarReserva(Cliente cliente, Mesa mesa, DateTime dataHora, int pessoas)
    {
        var reserva = new Reserva(_proximoIdReserva++, cliente, mesa, dataHora, pessoas);
        Reservas.Add(reserva);
        mesa.Status = StatusMesa.Reservada;
        return reserva;
    }

    public List<Reserva> AgendaDoDia(DateTime data)
        => Reservas
            .Where(r => r.DataHora.Date == data.Date)
            .OrderBy(r => r.DataHora)
            .ToList();

    internal void AjustarContadores(int proximaMesa, int proximaReserva)
    {
        if (proximaMesa > _proximoNumeroMesa) _proximoNumeroMesa = proximaMesa;
        if (proximaReserva > _proximoIdReserva) _proximoIdReserva = proximaReserva;
    }
}