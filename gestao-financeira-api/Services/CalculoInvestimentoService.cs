using GestaoFinanceira.DTOs;
using GestaoFinanceira.Model;

namespace GestaoFinanceira.Services
{
    /// <summary>
    /// Motor de calculo do historico de uma caixinha. Percorre as movimentacoes
    /// em ordem cronologica montando um "livro-razao" e derivando os indicadores.
    ///
    /// Regras por tipo:
    /// - Aporte:     aumenta capital investido e saldo.
    /// - Resgate:    diminui capital investido e saldo.
    /// - Rendimento: aumenta saldo e rendimento acumulado (nao mexe no capital).
    /// - Saldo:      foto do saldo total naquela data. O rendimento implicito e a
    ///               diferenca entre o saldo informado e o saldo esperado ate entao;
    ///               esse rendimento entra no acumulado. Assim, mesmo lancando o saldo
    ///               esporadicamente, o sistema reconcilia "quanto rendeu ate hoje".
    /// </summary>
    public class CalculoInvestimentoService
    {
        /// <summary>Resultado do processamento cronologico de uma caixinha.</summary>
        public sealed record ResultadoCaixinha(
            decimal SaldoAtual,
            decimal CapitalInvestido,
            decimal TotalAportado,
            decimal TotalResgatado,
            decimal RendimentoAcumulado,
            decimal RentabilidadePercentual,
            IReadOnlyDictionary<int, decimal> RendimentoDerivadoPorMovimentacao,
            IReadOnlyList<PontoEvolucaoDTO> Evolucao);

        public ResultadoCaixinha Processar(IEnumerable<Movimentacao> movimentacoes)
        {
            var ordenadas = movimentacoes
                .OrderBy(m => m.Data)
                .ThenBy(m => m.Id)
                .ToList();

            decimal saldo = 0m;
            decimal capital = 0m;
            decimal aportado = 0m;
            decimal resgatado = 0m;
            decimal rendimento = 0m;

            var rendimentoDerivado = new Dictionary<int, decimal>();
            var evolucao = new List<PontoEvolucaoDTO>();

            foreach (var mov in ordenadas)
            {
                switch (mov.Tipo)
                {
                    case TipoMovimentacao.Aporte:
                        capital += mov.Valor;
                        aportado += mov.Valor;
                        saldo += mov.Valor;
                        break;

                    case TipoMovimentacao.Resgate:
                        capital -= mov.Valor;
                        resgatado += mov.Valor;
                        saldo -= mov.Valor;
                        break;

                    case TipoMovimentacao.Rendimento:
                        rendimento += mov.Valor;
                        saldo += mov.Valor;
                        break;

                    case TipoMovimentacao.Saldo:
                        // Reconciliacao: o quanto o saldo informado excede o esperado
                        // ate agora e rendimento implicito acumulado no periodo.
                        var derivado = mov.Valor - saldo;
                        rendimento += derivado;
                        rendimentoDerivado[mov.Id] = derivado;
                        saldo = mov.Valor;
                        break;
                }

                evolucao.Add(new PontoEvolucaoDTO
                {
                    Data = mov.Data,
                    Saldo = saldo,
                    CapitalInvestido = capital,
                    RendimentoAcumulado = rendimento
                });
            }

            var rentabilidade = capital > 0m
                ? Math.Round(rendimento / capital * 100m, 2, MidpointRounding.AwayFromZero)
                : 0m;

            return new ResultadoCaixinha(
                SaldoAtual: saldo,
                CapitalInvestido: capital,
                TotalAportado: aportado,
                TotalResgatado: resgatado,
                RendimentoAcumulado: rendimento,
                RentabilidadePercentual: rentabilidade,
                RendimentoDerivadoPorMovimentacao: rendimentoDerivado,
                Evolucao: evolucao);
        }

        /// <summary>Monta o DTO de resumo de uma caixinha ja processada.</summary>
        public CaixinhaResumoDTO MontarResumo(Caixinha caixinha, ResultadoCaixinha resultado)
        {
            decimal? percentualMeta = null;
            if (caixinha.Meta is > 0m)
            {
                percentualMeta = Math.Round(resultado.SaldoAtual / caixinha.Meta.Value * 100m, 2, MidpointRounding.AwayFromZero);
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
                SaldoAtual = resultado.SaldoAtual,
                CapitalInvestido = resultado.CapitalInvestido,
                TotalAportado = resultado.TotalAportado,
                TotalResgatado = resultado.TotalResgatado,
                RendimentoAcumulado = resultado.RendimentoAcumulado,
                RentabilidadePercentual = resultado.RentabilidadePercentual,
                PercentualMeta = percentualMeta,
                QuantidadeMovimentacoes = caixinha.Movimentacoes.Count,
                UltimaMovimentacao = caixinha.Movimentacoes.Count > 0
                    ? caixinha.Movimentacoes.Max(m => m.Data)
                    : null
            };
        }
    }
}
