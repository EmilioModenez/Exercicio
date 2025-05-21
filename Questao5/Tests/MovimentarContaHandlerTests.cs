using Dapper;
using NSubstitute;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Handlers;
using System.Data;
using Xunit;

public class MovimentarContaHandlerTests
{
    [Fact]
    public async Task Deve_Gravar_Movimento_Quando_Conta_Existe_Ativa_E_Idempotencia_OK()
    {
        var dbConnection = Substitute.For<IDbConnection>();
        var handler = new MovimentarContaHandler(dbConnection);

        var command = new MovimentarContaCommand
        {
            ChaveIdempotencia = Guid.NewGuid().ToString(),
            IdContaCorrente = Guid.Parse("B6BAFC09-6967-ED11-A567-055DFA4A16C9"),
            Valor = 50,
            TipoMovimento = "C"
        };

        dbConnection
            .QuerySingleOrDefaultAsync<string>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(ci =>
            {
                var sql = ci.ArgAt<string>(0);
                if (sql.Contains("SELECT idempotencia"))
                    return Task.FromResult<string>(null); // Simula que não existe registro de idempotência
                if (sql.Contains("SELECT * FROM contacorrente"))
                    return Task.FromResult("{ \"idcontacorrente\": \"B6BAFC09-6967-ED11-A567-055DFA4A16C9\", \"nome\": \"Katherine Sanchez\", \"ativa\": 1 }"); // Conta ativa
                return Task.FromResult<string>("1");
            });

        dbConnection
            .ExecuteAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(1));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task MovimentarContaCorrente_ComDadosValidos_DeveRetornarIdMovimento()
    {
        var command = new MovimentarContaCommand
        {
            ChaveIdempotencia = Guid.NewGuid().ToString(),
            IdContaCorrente = Guid.Parse("B6BAFC09-6967-ED11-A567-055DFA4A16C9"),
            Valor = 50,
            TipoMovimento = "C"
        };

        var dbConnection = Substitute.For<IDbConnection>();
        var handler = new MovimentarContaHandler(dbConnection);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public async Task MovimentarContaCorrente_ContaInexistente_DeveLancarBusinessException()
    {
        var command = new MovimentarContaCommand
        {
            ChaveIdempotencia = Guid.NewGuid().ToString(),
            IdContaCorrente = Guid.NewGuid(),
            Valor = 100.00m,
            TipoMovimento = "C"
        };

        var dbConnection = Substitute.For<IDbConnection>();
        var handler = new MovimentarContaHandler(dbConnection);

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MovimentarContaCorrente_ValorInvalido_DeveLancarBusinessException()
    {
        var command = new MovimentarContaCommand
        {
            ChaveIdempotencia = Guid.NewGuid().ToString(),
            IdContaCorrente = Guid.Parse("B6BAFC09-6967-ED11-A567-055DFA4A16C9"),
            Valor = -50.00m,
            TipoMovimento = "C"
        };

        var dbConnection = Substitute.For<IDbConnection>();
        var handler = new MovimentarContaHandler(dbConnection);

        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}