using MediatR;
using Questao5.Domain.Entities;
using Questao5.Domain.Exceptions;
using Dapper;
using System.Data;
using System.Text.Json;
using Questao5.Application.Commands.Requests;

namespace Questao5.Application.Handlers;

public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand, string>
{
    private readonly IDbConnection _connection;

    public MovimentarContaHandler(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<string> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
    {
        // Verifica idempotência
        var resultadoExistente = await _connection.QueryFirstOrDefaultAsync<string>(
            "SELECT resultado FROM idempotencia WHERE chave_idempotencia = @chave",
            new { chave = request.ChaveIdempotencia });

        if (!string.IsNullOrEmpty(resultadoExistente))
        {
            var objeto = JsonSerializer.Deserialize<IdempotenciaResultado>(resultadoExistente);
            return objeto?.IdMovimento ?? string.Empty;
        }

        // Validações
        var conta = await _connection.QueryFirstOrDefaultAsync<ContaCorrente>(
            "SELECT * FROM contacorrente WHERE idcontacorrente = @id",
            new { id = request.IdContaCorrente });

        if (conta == null)
            throw new ContaInvalidaException();

        if (conta.Ativo == 0)
            throw new ContaInativaException();

        if (request.Valor <= 0)
            throw new ValorInvalidoException();

        if (request.TipoMovimento != "C" && request.TipoMovimento != "D")
            throw new TipoMovimentoInvalidoException();

        // Persistir movimento
        var idMovimento = Guid.NewGuid().ToString().ToUpper();
        var data = DateTime.Now.ToString("dd/MM/yyyy");

        await _connection.ExecuteAsync(
            @"INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
                  VALUES (@id, @conta, @data, @tipo, @valor)",
            new
            {
                id = idMovimento,
                conta = request.IdContaCorrente,
                data,
                tipo = request.TipoMovimento,
                valor = request.Valor
            });

        // Grava idempotência
        var resultadoJson = JsonSerializer.Serialize(new IdempotenciaResultado { IdMovimento = idMovimento });
        var requisicaoJson = JsonSerializer.Serialize(request);

        await _connection.ExecuteAsync(
            @"INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
                  VALUES (@chave, @req, @res)",
            new { chave = request.ChaveIdempotencia, req = requisicaoJson, res = resultadoJson });

        return idMovimento;
    }

    private class IdempotenciaResultado
    {
        public string IdMovimento { get; set; } = string.Empty;
    }
}