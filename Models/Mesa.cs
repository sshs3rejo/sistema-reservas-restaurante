namespace SistemaReservas.Models;

public class Mesa
{
    public int Numero { get; }
    public int Capacidade { get; }
    public string Localizacao { get; set; }
    public StatusMesa Status { get; set; } = StatusMesa.Disponivel;

    public Mesa(int numero, int capacidade, string localizacao)
    {
        Numero = numero;
        Capacidade = capacidade;
        Localizacao = localizacao;
    }

    public bool EstaDisponivel() => Status == StatusMesa.Disponivel;

    public override string ToString()
        => $"Mesa {Numero} ({Capacidade} lugares) - {Localizacao} - {Status}";
}