using GestaoFinanceira.DTOs;
using GestaoFinanceira.Model;

namespace GestaoFinanceira.Services
{
    public class CalculoInvestimentoService
    {
        /// <summary>Resultado do processamento cronologico de uma caixinha.</summary>
        public sealed record ResultadoCaixinha(
            decimal SaldoAtual,
            decimal CapitalInvestido,
            decimal TotalAportado,
            decimal TotalResgatado,
            decimal RendimentoAcumulado,
            decimal RentabilidadeMediaMensal,
            IReadOnlyDictionary<int, decimal> RendimentoDerivadoPorMovimentacao,
            IReadOnlyList<PontoEvolucaoDTO> Evolucao,
            IReadOnlyList<RendimentoMensalDTO> HistoricoMensal);

        public ResultadoCaixinha Processar(IEnumerable<Movimentacao> movimentacoes, decimal saldoInicial)
        {
            var ordenadas = movimentacoes
                .OrderBy(m => m.Data)
                .ThenBy(m => m.Id)
                .ToList();

            decimal saldo = saldoInicial;
            decimal capital = 0m;
            decimal aportado = 0m;
            decimal resgatado = 0m;
            decimal rendimento = 0m;

            var rendimentoDerivado = new Dictionary<int, decimal>();
            var evolucao = new List<PontoEvolucaoDTO>();
            var rendimentoPorMes = new SortedDictionary<DateTime, decimal>();

            foreach (var mov in ordenadas)
            {
                decimal rendimentoDoEvento = 0m;

                switch (mov.Tipo)
                {
                    case TipoMovimentacao.Aporte:
                        aportado += mov.Valor;
                        capital += mov.Valor;
                        saldo += mov.Valor;
                        break;

                    case TipoMovimentacao.Resgate:
                        resgatado += mov.Valor;
                        saldo -= mov.Valor;
                        break;

                    case TipoMovimentacao.Rendimento:
                        rendimento += mov.Valor;
                        saldo += mov.Valor;
                        rendimentoDoEvento = mov.Valor;
                        break;

                    case TipoMovimentacao.Saldo:
                        var delta = mov.Valor - saldo;
                        rendimento += delta;
                        saldo += delta;
                        rendimentoDoEvento = delta;
                        rendimentoDerivado[mov.Id] = delta;
                        break;
                }

                if (rendimentoDoEvento != 0m)
                {
                    var mesRef = new DateTime(mov.Data.Year, mov.Data.Month, 1);
                    rendimentoPorMes.TryGetValue(mesRef, out var acumMes);
                    rendimentoPorMes[mesRef] = acumMes + rendimentoDoEvento;
                }

                evolucao.Add(new PontoEvolucaoDTO
                {
                    Data = mov.Data,
                    Saldo = saldo,
                    CapitalInvestido = capital,
                    RendimentoAcumulado = rendimento
                });
            }

            var historicoMensal = rendimentoPorMes
                .Select(kv => new RendimentoMensalDTO
                {
                    Ano = kv.Key.Year,
                    Mes = kv.Key.Month,
                    Rendimento = kv.Value
                })
                .ToList();

            // Media mensal em R$ considerando apenas os meses que tiveram rendimento.
            var mesesComRendimento = historicoMensal.Count(h => h.Rendimento != 0m);
            var rentabilidadeMediaMensal = mesesComRendimento > 0
                ? Math.Round(rendimento / mesesComRendimento, 2, MidpointRounding.AwayFromZero)
                : 0m;

            return new ResultadoCaixinha(
                SaldoAtual: saldo,
                CapitalInvestido: capital,
                TotalAportado: aportado,
                TotalResgatado: resgatado,
                RendimentoAcumulado: rendimento,
                RentabilidadeMediaMensal: rentabilidadeMediaMensal,
                RendimentoDerivadoPorMovimentacao: rendimentoDerivado,
                Evolucao: evolucao,
                HistoricoMensal: historicoMensal);
        }

        /// <summary>Monta o DTO de resumo de uma caixinha ja processada.</summary>
        public CaixinhaResumoDTO MontarResumo(Caixinha caixinha, ResultadoCaixinha resultado)
        {
            decimal saldoAtual = resultado.SaldoAtual;
            decimal? percentualMeta = null;
            if (caixinha.Meta is > 0m)
            {
                percentualMeta = Math.Round(saldoAtual / caixinha.Meta.Value * 100m, 2, MidpointRounding.AwayFromZero);
            }

            return new CaixinhaResumoDTO
            {
                Id = caixinha.Id,
                Nome = caixinha.Nome,
                Descricao = caixinha.Descricao,
                Cor = caixinha.Cor,
                Icone = caixinha.Icone,
                Meta = caixinha.Meta,
                Arquivada = caixinha.Arquivada,
                CriadaEm = caixinha.CriadaEm,
                SaldoAtual = saldoAtual,
                CapitalInvestido = resultado.CapitalInvestido,
                TotalAportado = resultado.TotalAportado,
                TotalResgatado = resultado.TotalResgatado,
                RendimentoAcumulado = resultado.RendimentoAcumulado,
                RentabilidadeMediaMensal = resultado.RentabilidadeMediaMensal,
                HistoricoMensal = resultado.HistoricoMensal.ToList(),
                PercentualMeta = percentualMeta,
                QuantidadeMovimentacoes = caixinha.Movimentacoes.Count,
                UltimaMovimentacao = caixinha.Movimentacoes.Count > 0
                    ? caixinha.Movimentacoes.Max(m => m.Data)
                    : null,
                SaldoInicial = caixinha.SaldoInicial
            };
        }
    }
}
