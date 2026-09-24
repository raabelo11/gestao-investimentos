using GestaoFinanceira.Data;
using GestaoFinanceira.DTOs;
using GestaoFinanceira.Model;
using GestaoFinanceira.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceira.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaixinhasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CalculoInvestimentoService _calculo;

        public CaixinhasController(AppDbContext db, CalculoInvestimentoService calculo)
        {
            _db = db;
            _calculo = calculo;
        }

        /// <summary>Lista as caixinhas com seus indicadores. Por padrao oculta arquivadas.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CaixinhaResumoDTO>>> Listar([FromQuery] bool incluirArquivadas = false)
        {
            var query = _db.Caixinhas.Include(c => c.Movimentacoes).AsQueryable();
            if (!incluirArquivadas)
            {
                query = query.Where(c => !c.Arquivada);
            }

            var caixinhas = await query.AsNoTracking().ToListAsync();

            var resumos = caixinhas
                .Select(c => _calculo.MontarResumo(c, _calculo.Processar(c.Movimentacoes, c.SaldoInicial)))
                .OrderByDescending(r => r.SaldoAtual)
                .ToList();

            return Ok(resumos);
        }

        /// <summary>Detalha uma caixinha com seus indicadores calculados.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CaixinhaResumoDTO>> ObterPorId(int id)
        {
            var caixinha = await _db.Caixinhas
                .Include(c => c.Movimentacoes)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caixinha is null)
            {
                return NotFound($"Caixinha {id} nao encontrada.");
            }

            var resultado = _calculo.Processar(caixinha.Movimentacoes, caixinha.SaldoInicial);
            return Ok(_calculo.MontarResumo(caixinha, resultado));
        }

        /// <summary>Cria uma nova caixinha.</summary>
        [HttpPost]
        public async Task<ActionResult<CaixinhaResumoDTO>> Criar([FromBody] CaixinhaInputDTO input)
        {
            var caixinha = new Caixinha
            {
                Nome = input.Nome.Trim(),
                Descricao = input.Descricao?.Trim(),
                Cor = input.Cor,
                Icone = input.Icone,
                Meta = input.Meta,
                CriadaEm = DateTime.UtcNow,
                SaldoInicial = input.SaldoInicial,
            };

            _db.Caixinhas.Add(caixinha);
            await _db.SaveChangesAsync();

            var resultado = _calculo.Processar(caixinha.Movimentacoes, caixinha.SaldoInicial);
            var resumo = _calculo.MontarResumo(caixinha, resultado);
            return CreatedAtAction(nameof(ObterPorId), new { id = caixinha.Id }, resumo);
        }

        /// <summary>Atualiza os dados de uma caixinha.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CaixinhaResumoDTO>> Atualizar(int id, [FromBody] CaixinhaInputDTO input)
        {
            var caixinha = await _db.Caixinhas
                .Include(c => c.Movimentacoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caixinha is null)
            {
                return NotFound($"Caixinha {id} nao encontrada.");
            }

            caixinha.Nome = input.Nome.Trim();
            caixinha.Descricao = input.Descricao?.Trim();
            caixinha.Cor = input.Cor;
            caixinha.Icone = input.Icone;
            caixinha.Meta = input.Meta;

            await _db.SaveChangesAsync();

            var resultado = _calculo.Processar(caixinha.Movimentacoes, caixinha.SaldoInicial);
            return Ok(_calculo.MontarResumo(caixinha, resultado));
        }

        /// <summary>Arquiva ou desarquiva uma caixinha (mantem o historico).</summary>
        [HttpPatch("{id:int}/arquivamento")]
        public async Task<ActionResult<CaixinhaResumoDTO>> AlternarArquivamento(int id, [FromQuery] bool arquivar = true)
        {
            var caixinha = await _db.Caixinhas
                .Include(c => c.Movimentacoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caixinha is null)
            {
                return NotFound($"Caixinha {id} nao encontrada.");
            }

            caixinha.Arquivada = arquivar;
            await _db.SaveChangesAsync();

            var resultado = _calculo.Processar(caixinha.Movimentacoes, caixinha.SaldoInicial);
            return Ok(_calculo.MontarResumo(caixinha, resultado));
        }

        /// <summary>Exclui a caixinha e todo o seu historico.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var caixinha = await _db.Caixinhas.FirstOrDefaultAsync(c => c.Id == id);
            if (caixinha is null)
            {
                return NotFound($"Caixinha {id} nao encontrada.");
            }

            _db.Caixinhas.Remove(caixinha);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
