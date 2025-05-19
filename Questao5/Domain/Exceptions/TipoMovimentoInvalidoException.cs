namespace Questao5.Domain.Exceptions;

public class TipoMovimentoInvalidoException : Exception
{
    public TipoMovimentoInvalidoException() : base("Tipo de movimentação inválido. Use 'C' para crédito ou 'D' para débito.") { }
}
