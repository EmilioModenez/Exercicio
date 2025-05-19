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
        // Arrange
        var dbConnection = Substitute.For<IDbConnection>();
        var handler = new MovimentarContaHandler(dbConnection);

        var command = new MovimentarContaCommand
        {
            ChaveIdempotencia = Guid.NewGuid().ToString(),
            IdContaCorrente = Guid.NewGuid(),
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
                    return Task.FromResult("{ \"idcontacorrente\": \"123\", \"nome\": \"Conta 123\", \"ativa\": 1 }"); // Conta ativa
                return Task.FromResult<string>("1");
            });

        dbConnection
            .ExecuteAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(1));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(string.IsNullOrEmpty(result));
    }
}
