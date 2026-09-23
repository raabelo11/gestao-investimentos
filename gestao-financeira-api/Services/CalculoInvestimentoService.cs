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
    /// - Resgate:    diminui APENAS o saldo. O capital investido nao diminui
    ///               (o que ja foi investido continua contabilizado como investido)
    ///               e o rendimento nao e afetado.
    /// - Rendimento: aumenta saldo e rendimento acumulado (nao mexe no capital).
    /// - Saldo:      foto do saldo total naquela data. O rendimento que ela
    ///               representa e CONGELADO no momento do lancamento (campo
    ///               <see cref="Movimentacao.RendimentoCongelado"/>) e nunca mais
    ///               recalculado. Assim, aportes/resgates (mesmo retroativos) jamais
    ///               alteram o rendimento. A foto apenas reposiciona o saldo para o
    ///               valor informado. (Lancamentos legados sem valor congelado caem
    ///               num fallback que deriva o rendimento uma unica vez.)
    ///
    /// Rentabilidade: media mensal em R$ do quanto rendeu por mes, considerando
    /// apenas os meses que tiveram rendimento (ver <see cref="RentabilidadeMediaMensal"/>).
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
            decimal RentabilidadeMediaMensal,
            IReadOnlyDictionary<int, decimal> RendimentoDerivadoPorMovimentacao,
            IReadOnlyList<PontoEvolucaoDTO> Evolucao,
            IReadOnlyList<RendimentoMensalDTO> HistoricoMensal);

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
            // Rendimento (explicito + derivado) somado por mes de competencia.
            var rendimentoPorMes = new SortedDictionary<DateTime, decimal>();

            foreach (var mov in ordenadas)
            {
                decimal rendimentoDoEvento = 0m;

                switch (mov.Tipo)
                {
                    case TipoMovimentacao.Aporte:
                        capital += mov.Valor;
                        aportado += mov.Valor;
                        saldo = saldo + mov.Valor;
                        break;

                    case TipoMovimentacao.Resgate:
                        // Resgate mexe APENAS no saldo: o capital investido nao diminui
                        // e o rendimento nao e afetado.
                        resgatado += mov.Valor;
                        saldo = saldo - mov.Valor;
                        break;

                    case TipoMovimentacao.Rendimento:
                        rendimento += mov.Valor;
                        rendimentoDoEvento = mov.Valor;
                        saldo += mov.Valor;
                        break;

                    case TipoMovimentacao.Saldo:
                        // O rendimento desta foto e o valor CONGELADO no lancamento.
                        // Nunca recalculamos a partir do saldo esperado, para que
                        // aportes/resgates (mesmo retroativos) nao alterem o rendimento.
                        // Fallback so para dados legados sem valor congelado.
                        var derivado = mov.RendimentoCongelado ?? (mov.Valor - saldo);
                        rendimento += derivado;
                        rendimentoDoEvento = derivado;
                        rendimentoDerivado[mov.Id] = derivado;
                        saldo = mov.Valor;
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

        /// <summary>
        /// Calcula o rendimento a CONGELAR para uma foto de saldo (tipo Saldo),
        /// no momento em que ela e salva: saldo informado menos o saldo esperado
        /// considerando apenas as demais movimentacoes ate a data da foto.
        /// </summary>
        /// <param name="saldoInformado">Saldo total digitado pelo usuario na foto.</param>
        /// <param name="dataFoto">Data de competencia da foto.</param>
        /// <param name="idFoto">Id da foto (0 se ainda nao persistida) para desempate/exclusao.</param>
        /// <param name="demaisMovimentacoes">Todas as movimentacoes da caixinha (a propria foto e ignorada).</param>
        public decimal CalcularRendimentoCongelado(
            decimal saldoInformado,
            DateTime dataFoto,
            int idFoto,
            IEnumerable<Movimentacao> demaisMovimentacoes)
        {
            var anteriores = demaisMovimentacoes
                .Where(m => m.Id != idFoto)
                .Where(m => m.Data < dataFoto || (m.Data == dataFoto && m.Id < idFoto))
                .OrderBy(m => m.Data)
                .ThenBy(m => m.Id);

            decimal saldoEsperado = 0m;
            foreach (var mov in anteriores)
            {
                switch (mov.Tipo)
                {
                    case TipoMovimentacao.Aporte:
                        saldoEsperado += mov.Valor;
                        break;
                    case TipoMovimentacao.Resgate:
                        saldoEsperado -= mov.Valor;
                        break;
                    case TipoMovimentacao.Rendimento:
                        saldoEsperado += mov.Valor;
                        break;
                    case TipoMovimentacao.Saldo:
                        saldoEsperado = mov.Valor;
                        break;
                }
            }

            return saldoInformado - saldoEsperado;
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
                RentabilidadeMediaMensal = resultado.RentabilidadeMediaMensal,
                HistoricoMensal = resultado.HistoricoMensal.ToList(),
                PercentualMeta = percentualMeta,
                QuantidadeMovimentacoes = caixinha.Movimentacoes.Count,
                UltimaMovimentacao = caixinha.Movimentacoes.Count > 0
                    ? caixinha.Movimentacoes.Max(m => m.Data)
                    : null
            };
        }
    }
}
