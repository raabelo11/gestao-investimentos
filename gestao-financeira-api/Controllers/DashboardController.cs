using GestaoFinanceira.Data;
using GestaoFinanceira.DTOs;
using GestaoFinanceira.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceira.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CalculoInvestimentoService _calculo;

        public DashboardController(AppDbContext db, CalculoInvestimentoService calculo)
        {
            _db = db;
            _calculo = calculo;
        }

        /// <summary>Visao consolidada do patrimonio: totais, caixinhas e evolucao.</summary>
        [HttpGet]
        public async Task<ActionResult<DashboardDTO>> Obter()
        {
            var caixinhas = await _db.Caixinhas
                .Include(c => c.Movimentacoes)
                .Where(c => !c.Arquivada)
                .AsNoTracking()
                .ToListAsync();

            var resumos = new List<CaixinhaResumoDTO>();
            var pontosPorData = new SortedDictionary<DateTime, PontoEvolucaoDTO>();

            foreach (var caixinha in caixinhas)
            {
                var resultado = _calculo.Processar(caixinha.Movimentacoes);
                resumos.Add(_calculo.MontarResumo(caixinha, resultado));
                AcumularEvolucao(pontosPorData, resultado.Evolucao);
            }

            var dashboard = new DashboardDTO
            {
                QuantidadeCaixinhas = resumos.Count,
                PatrimonioTotal = resumos.Sum(r => r.SaldoAtual),
                CapitalInvestidoTotal = resumos.Sum(r => r.CapitalInvestido),
                RendimentoAcumuladoTotal = resumos.Sum(r => r.RendimentoAcumulado),
                TotalAportado = resumos.Sum(r => r.TotalAportado),
                TotalResgatado = resumos.Sum(r => r.TotalResgatado),
                Caixinhas = resumos.OrderByDescending(r => r.SaldoAtual).ToList(),
                EvolucaoPatrimonio = pontosPorData.Values.ToList()
            };

            dashboard.RentabilidadePercentualGeral = dashboard.CapitalInvestidoTotal > 0m
                ? Math.Round(dashboard.RendimentoAcumuladoTotal / dashboard.CapitalInvestidoTotal * 100m, 2, MidpointRounding.AwayFromZero)
                : 0m;

            return Ok(dashboard);
        }

        /// <summary>
        /// Consolida a evolucao de varias caixinhas em uma unica serie de patrimonio.
        /// Cada data acumula o ultimo estado conhecido de cada caixinha.
        /// </summary>
        private static void AcumularEvolucao(
            SortedDictionary<DateTime, PontoEvolucaoDTO> destino,
            IReadOnlyList<PontoEvolucaoDTO> origem)
        {
            foreach (var ponto in origem)
            {
                var dia = ponto.Data.Date;
                if (destino.TryGetValue(dia, out var existente))
                {
                    existente.Saldo += ponto.Saldo;
                    existente.CapitalInvestido += ponto.CapitalInvestido;
                    existente.RendimentoAcumulado += ponto.RendimentoAcumulado;
                }
                else
                {
                    destino[dia] = new PontoEvolucaoDTO
                    {
                        Data = dia,
                        Saldo = ponto.Saldo,
                        CapitalInvestido = ponto.CapitalInvestido,
                        RendimentoAcumulado = ponto.RendimentoAcumulado
                    };
                }
            }
        }
    }
}
