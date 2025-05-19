using MediatR;

namespace Questao5.Application.Commands.Requests;

public class MovimentarContaCommand : IRequest<string>
{
    public string ChaveIdempotencia { get; set; } = string.Empty;
    public Guid IdContaCorrente { get; set; }
    public decimal Valor { get; set; }
    public string TipoMovimento { get; set; } = string.Empty;
}
