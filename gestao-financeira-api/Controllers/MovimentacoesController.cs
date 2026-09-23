using GestaoFinanceira.Data;
using GestaoFinanceira.DTOs;
using GestaoFinanceira.Model;
using GestaoFinanceira.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceira.Controllers
{
    [ApiController]
    [Route("api/caixinhas/{caixinhaId:int}/movimentacoes")]
    public class MovimentacoesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CalculoInvestimentoService _calculo;

        public MovimentacoesController(AppDbContext db, CalculoInvestimentoService calculo)
        {
            _db = db;
            _calculo = calculo;
        }

        /// <summary>Lista as movimentacoes de uma caixinha (mais recentes primeiro),
        /// com o rendimento derivado das fotos de saldo.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimentacaoDTO>>> Listar(int caixinhaId)
        {
            if (!await _db.Caixinhas.AnyAsync(c => c.Id == caixinhaId))
            {
                return NotFound($"Caixinha {caixinhaId} nao encontrada.");
            }

            var movimentacoes = await _db.Movimentacoes
                .Where(m => m.CaixinhaId == caixinhaId)
                .AsNoTracking()
                .ToListAsync();

            var resultado = _calculo.Processar(movimentacoes);

            var dtos = movimentacoes
                .OrderByDescending(m => m.Data)
                .ThenByDescending(m => m.Id)
                .Select(m => MapearParaDTO(m, resultado))
                .ToList();

            return Ok(dtos);
        }

        /// <summary>Registra uma nova movimentacao na caixinha.</summary>
        [HttpPost]
        public async Task<ActionResult<MovimentacaoDTO>> Criar(int caixinhaId, [FromBody] MovimentacaoInputDTO input)
        {
            if (!await _db.Caixinhas.AnyAsync(c => c.Id == caixinhaId))
            {
                return NotFound($"Caixinha {caixinhaId} nao encontrada.");
            }

            var movimentacao = new Movimentacao
            {
                CaixinhaId = caixinhaId,
                Tipo = input.Tipo,
                Valor = input.Valor,
                Data = input.Data,
                Observacao = input.Observacao?.Trim(),
                CriadaEm = DateTime.UtcNow
            };

            _db.Movimentacoes.Add(movimentacao);
            await _db.SaveChangesAsync();

            // Foto de saldo: congela o rendimento derivado agora, para que
            // aportes/resgates futuros ou retroativos nunca o recalculem.
            await CongelarRendimentoSeFotoDeSaldoAsync(movimentacao);

            var todas = await _db.Movimentacoes
                .Where(m => m.CaixinhaId == caixinhaId)
                .AsNoTracking()
                .ToListAsync();
            var resultado = _calculo.Processar(todas);

            return CreatedAtAction(nameof(Listar), new { caixinhaId },
                MapearParaDTO(movimentacao, resultado));
        }

        /// <summary>Atualiza uma movimentacao existente.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<MovimentacaoDTO>> Atualizar(int caixinhaId, int id, [FromBody] MovimentacaoInputDTO input)
        {
            var movimentacao = await _db.Movimentacoes
                .FirstOrDefaultAsync(m => m.Id == id && m.CaixinhaId == caixinhaId);

            if (movimentacao is null)
            {
                return NotFound($"Movimentacao {id} nao encontrada na caixinha {caixinhaId}.");
            }

            movimentacao.Tipo = input.Tipo;
            movimentacao.Valor = input.Valor;
            movimentacao.Data = input.Data;
            movimentacao.Observacao = input.Observacao?.Trim();

            // Se deixou de ser foto de saldo, descarta o rendimento congelado.
            if (movimentacao.Tipo != TipoMovimentacao.Saldo)
            {
                movimentacao.RendimentoCongelado = null;
            }

            await _db.SaveChangesAsync();

            // Recongela o rendimento se (ainda) e uma foto de saldo.
            await CongelarRendimentoSeFotoDeSaldoAsync(movimentacao);

            var todas = await _db.Movimentacoes
                .Where(m => m.CaixinhaId == caixinhaId)
                .AsNoTracking()
                .ToListAsync();
            var resultado = _calculo.Processar(todas);

            return Ok(MapearParaDTO(movimentacao, resultado));
        }

        /// <summary>Exclui uma movimentacao.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int caixinhaId, int id)
        {
            var movimentacao = await _db.Movimentacoes
                .FirstOrDefaultAsync(m => m.Id == id && m.CaixinhaId == caixinhaId);

            if (movimentacao is null)
            {
                return NotFound($"Movimentacao {id} nao encontrada na caixinha {caixinhaId}.");
            }

            _db.Movimentacoes.Remove(movimentacao);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Se a movimentacao for uma foto de saldo, calcula e persiste o rendimento
        /// congelado (saldo informado - saldo esperado ate a data da foto). Deve ser
        /// chamada DEPOIS que a movimentacao ja tem Id definitivo.
        /// </summary>
        private async Task CongelarRendimentoSeFotoDeSaldoAsync(Movimentacao foto)
        {
            if (foto.Tipo != TipoMovimentacao.Saldo)
            {
                return;
            }

            var demais = await _db.Movimentacoes
                .Where(m => m.CaixinhaId == foto.CaixinhaId && m.Id != foto.Id)
                .AsNoTracking()
                .ToListAsync();

            foto.RendimentoCongelado = _calculo.CalcularRendimentoCongelado(
                foto.Valor, foto.Data, foto.Id, demais);

            await _db.SaveChangesAsync();
        }

        private static MovimentacaoDTO MapearParaDTO(Movimentacao m, CalculoInvestimentoService.ResultadoCaixinha resultado)
        {
            decimal? derivado = null;
            if (m.Tipo == TipoMovimentacao.Saldo &&
                resultado.RendimentoDerivadoPorMovimentacao.TryGetValue(m.Id, out var valor))
            {
                derivado = valor;
            }

            return new MovimentacaoDTO
            {
                Id = m.Id,
                CaixinhaId = m.CaixinhaId,
                Tipo = m.Tipo,
                TipoDescricao = m.Tipo.ToString(),
                Valor = m.Valor,
                Data = m.Data,
                Observacao = m.Observacao,
                CriadaEm = m.CriadaEm,
                RendimentoDerivado = derivado
            };
        }
    }
}
