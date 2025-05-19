using Dapper;
using MediatR;
using Questao5.Application.Queries;
using Questao5.Domain.Entities;
using Questao5.Domain.Exceptions;
using System.Data;

namespace Questao5.Application.Handlers;

public class ObterSaldoContaHandler : IRequestHandler<ObterSaldoContaQuery, SaldoContaCorrenteDto>
{
    private readonly IDbConnection _connection;

    public ObterSaldoContaHandler(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<SaldoContaCorrenteDto> Handle(ObterSaldoContaQuery request, CancellationToken cancellationToken)
    {
        // Busca a conta corrente
        var conta = await _connection.QueryFirstOrDefaultAsync<ContaCorrente>(
            "SELECT * FROM contacorrente WHERE idcontacorrente = @id",
            new { id = request.IdContaCorrente });

        if (conta == null)
            throw new ContaInvalidaException();

        if (conta.Ativo == 0)
            throw new ContaInativaException();

        // Calcula saldo
        var resultado = await _connection.QueryFirstOrDefaultAsync<(decimal? credito, decimal? debito)>(
            @"SELECT 
                    SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE 0 END) AS credito,
                    SUM(CASE WHEN tipomovimento = 'D' THEN valor ELSE 0 END) AS debito
                  FROM movimento
                  WHERE idcontacorrente = @id",
            new { id = request.IdContaCorrente });

        decimal saldo = (resultado.credito ?? 0) - (resultado.debito ?? 0);

        return new SaldoContaCorrenteDto
        {
            Numero = conta.Numero,
            Nome = conta.Nome,
            DataConsulta = DateTime.Now,
            Saldo = saldo
        };
    }
}
