namespace Questao5.Domain.Exceptions;

public class ValorInvalidoException : Exception
{
    public ValorInvalidoException() : base("O valor da movimentação deve ser maior que zero.") { }
}
