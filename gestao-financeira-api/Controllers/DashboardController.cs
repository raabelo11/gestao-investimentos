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
            var evolucoesPorCaixinha = new List<IReadOnlyList<PontoEvolucaoDTO>>();
            var rendimentoPorMes = new SortedDictionary<DateTime, decimal>();

            foreach (var caixinha in caixinhas)
            {
                var resultado = _calculo.Processar(caixinha.Movimentacoes, caixinha.SaldoInicial);
                resumos.Add(_calculo.MontarResumo(caixinha, resultado));
                evolucoesPorCaixinha.Add(resultado.Evolucao);
                AcumularHistoricoMensal(rendimentoPorMes, resultado.HistoricoMensal);
            }

            var historicoMensal = rendimentoPorMes
                .Select(kv => new RendimentoMensalDTO { Ano = kv.Key.Year, Mes = kv.Key.Month, Rendimento = kv.Value })
                .ToList();

            var dashboard = new DashboardDTO
            {
                QuantidadeCaixinhas = resumos.Count,
                PatrimonioTotal = resumos.Sum(r => r.SaldoAtual),
                CapitalInvestidoTotal = resumos.Sum(r => r.CapitalInvestido),
                RendimentoAcumuladoTotal = resumos.Sum(r => r.RendimentoAcumulado),
                TotalAportado = resumos.Sum(r => r.TotalAportado),
                TotalResgatado = resumos.Sum(r => r.TotalResgatado),
                Caixinhas = resumos.OrderByDescending(r => r.SaldoAtual).ToList(),
                EvolucaoPatrimonio = ConsolidarEvolucao(evolucoesPorCaixinha),
                HistoricoMensal = historicoMensal
            };

            // Rentabilidade consolidada: media em R$ por mes que teve rendimento.
            var mesesComRendimento = historicoMensal.Count(h => h.Rendimento != 0m);
            dashboard.RentabilidadeMediaMensalGeral = mesesComRendimento > 0
                ? Math.Round(dashboard.RendimentoAcumuladoTotal / mesesComRendimento, 2, MidpointRounding.AwayFromZero)
                : 0m;

            return Ok(dashboard);
        }

        /// <summary>Soma o rendimento mensal de uma caixinha ao acumulado geral.</summary>
        private static void AcumularHistoricoMensal(
            SortedDictionary<DateTime, decimal> destino,
            IReadOnlyList<RendimentoMensalDTO> origem)
        {
            foreach (var mes in origem)
            {
                var chave = new DateTime(mes.Ano, mes.Mes, 1);
                destino.TryGetValue(chave, out var acum);
                destino[chave] = acum + mes.Rendimento;
            }
        }

        /// <summary>
        /// Consolida a evolucao de varias caixinhas em uma unica serie de patrimonio.
        /// Para cada dia da uniao de datas, soma o ULTIMO estado conhecido de cada
        /// caixinha ate aquele dia (forward-fill), evitando que a linha "caia" nos
        /// dias em que uma caixinha nao teve movimentacao.
        /// </summary>
        private static List<PontoEvolucaoDTO> ConsolidarEvolucao(
            IReadOnlyList<IReadOnlyList<PontoEvolucaoDTO>> evolucoes)
        {
            var datas = evolucoes
                .SelectMany(e => e.Select(p => p.Data.Date))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var consolidado = new List<PontoEvolucaoDTO>(datas.Count);

            foreach (var dia in datas)
            {
                decimal saldo = 0m, capital = 0m, rendimento = 0m;

                foreach (var evolucao in evolucoes)
                {
                    // Ultimo ponto da caixinha ate (e inclusive) este dia.
                    PontoEvolucaoDTO? ultimo = null;
                    foreach (var ponto in evolucao)
                    {
                        if (ponto.Data.Date <= dia)
                        {
                            ultimo = ponto;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (ultimo is not null)
                    {
                        saldo += ultimo.Saldo;
                        capital += ultimo.CapitalInvestido;
                        rendimento += ultimo.RendimentoAcumulado;
                    }
                }

                consolidado.Add(new PontoEvolucaoDTO
                {
                    Data = dia,
                    Saldo = saldo,
                    CapitalInvestido = capital,
                    RendimentoAcumulado = rendimento
                });
            }

            return consolidado;
        }
    }
}
