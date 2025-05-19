using MediatR;

namespace Questao5.Application.Queries;

public class ObterSaldoContaQuery : IRequest<SaldoContaCorrenteDto>
{
    public Guid IdContaCorrente { get; }

    public ObterSaldoContaQuery(Guid idContaCorrente)
    {
        IdContaCorrente = idContaCorrente;
    }
}

public class SaldoContaCorrenteDto
{
    public int Numero { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataConsulta { get; set; }
    public decimal Saldo { get; set; }
}
