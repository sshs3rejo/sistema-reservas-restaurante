namespace SistemaReservas.Models;

public class Cliente : Pessoa
{
    public int QtdVisitas { get; private set; }

    public Cliente(int id, string nome, string telefone, string cpf, string email)
        : base(id, nome, telefone, cpf, email)
    {
    }

    public void RegistrarVisita() => QtdVisitas++;

    internal void RestaurarQtdVisitas(int qtd) => QtdVisitas = qtd;

    public string NivelFidelidade
        => QtdVisitas >= 10 ? "VIP" : QtdVisitas >= 5 ? "Regular" : "Novo";

    public override string Tipo() => "Cliente";

    public override string ExibirInformacoes()
        => $"{base.ExibirInformacoes()} | Fidelidade: {NivelFidelidade}";
}