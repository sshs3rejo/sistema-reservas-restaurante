namespace SistemaReservas.Models;

public abstract class Pessoa
{
    public int Id { get; protected set; }
    public string Nome { get; set; }
    public string Telefone { get; set; }
    public string Cpf { get; set; }
    public string Email { get; set; }

    protected Pessoa(int id, string nome, string telefone, string cpf, string email)
    {
        Id = id;
        Nome = nome;
        Telefone = telefone;
        Cpf = cpf;
        Email = email;
    }

    public abstract string Tipo();

    public virtual string ExibirInformacoes()
        => $"{Id} - {Nome} | Tel: {Telefone} | CPF: {Cpf} | {Email}";
}