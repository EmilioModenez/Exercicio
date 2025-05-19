using MediatR;
using Microsoft.AspNetCore.Mvc;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Queries;
using Questao5.Domain.Exceptions;

namespace Questao5.Infrastructure.Services.Controllers
{
    [ApiController]
    //[Route("api/v1/[controller]")]
    [Route("api/v1/contacorrente")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContaCorrenteController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost("movimento")]
        public async Task<IActionResult> MovimentarConta([FromBody] MovimentarContaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return Ok(new { idMovimento = resultado });
            }
            catch (ContaInvalidaException)
            {
                return BadRequest(new { erro = "Conta inválida", tipo = "INVALID_ACCOUNT" });
            }
            catch (ContaInativaException)
            {
                return BadRequest(new { erro = "Conta inativa", tipo = "INACTIVE_ACCOUNT" });
            }
            catch (ValorInvalidoException)
            {
                return BadRequest(new { erro = "Valor inválido", tipo = "INVALID_VALUE" });
            }
            catch (TipoMovimentoInvalidoException)
            {
                return BadRequest(new { erro = "Tipo de movimento inválido", tipo = "INVALID_TYPE" });
            }
        }

        [HttpGet("{idContaCorrente}/saldo")]
        public async Task<IActionResult> ObterSaldo([FromRoute] Guid idContaCorrente)
        {
            try
            {
                var query = new ObterSaldoContaQuery(idContaCorrente);
                var resultado = await _mediator.Send(query);
                return Ok(resultado);
            }
            catch (ContaInvalidaException)
            {
                return BadRequest(new { erro = "Conta inválida", tipo = "INVALID_ACCOUNT" });
            }
            catch (ContaInativaException)
            {
                return BadRequest(new { erro = "Conta inativa", tipo = "INACTIVE_ACCOUNT" });
            }
        }

    }
}
