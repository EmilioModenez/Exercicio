namespace Questao5.Domain.Exceptions;

public class ContaInativaException : Exception
{
    public ContaInativaException() : base("A conta corrente está inativa.") { }
}
