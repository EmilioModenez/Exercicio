namespace Questao5.Domain.Exceptions;

public class ContaInvalidaException : Exception
{
    public ContaInvalidaException() : base("Conta corrente não encontrada.") { }
}
