using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceira.DTOs
{
    /// <summary>Payload para criar ou atualizar uma caixinha.</summary>
    public class CaixinhaInputDTO
    {
        [Required(ErrorMessage = "O nome e obrigatorio.")]
        [MaxLength(80)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(280)]
        public string? Descricao { get; set; }

        [MaxLength(9)]
        public string Cor { get; set; } = "#4F46E5";

        [MaxLength(40)]
        public string Icone { get; set; } = "savings";

        [Range(0, double.MaxValue, ErrorMessage = "A meta nao pode ser negativa.")]
        public decimal? Meta { get; set; }

        public decimal SaldoInicial { get; set; }
    }

    /// <summary>Representacao de leitura de uma caixinha com seus indicadores calculados.</summary>
    public class CaixinhaResumoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string Cor { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
        public decimal? Meta { get; set; }
        public bool Arquivada { get; set; }
        public DateTime CriadaEm { get; set; }

        /// <summary>Saldo atual da caixinha (capital + rendimentos - resgates).</summary>
        public decimal SaldoAtual { get; set; }

        /// <summary>Capital efetivamente investido (aportes - resgates).</summary>
        public decimal CapitalInvestido { get; set; }

        /// <summary>Total de aportes ja realizados.</summary>
        public decimal TotalAportado { get; set; }

        /// <summary>Total de resgates ja realizados.</summary>
        public decimal TotalResgatado { get; set; }

        /// <summary>Rendimento acumulado (explicito + derivado de snapshots de saldo).</summary>
        public decimal RendimentoAcumulado { get; set; }

        /// <summary>
        /// Rentabilidade = media, em R$, do quanto rendeu por mes, considerando
        /// apenas os meses que tiveram rendimento.
        /// </summary>
        public decimal RentabilidadeMediaMensal { get; set; }

        /// <summary>Rendimento em R$ mes a mes (para o historico mensal).</summary>
        public List<RendimentoMensalDTO> HistoricoMensal { get; set; } = new();

        /// <summary>Percentual da meta atingido, quando ha meta definida.</summary>
        public decimal? PercentualMeta { get; set; }

        public int QuantidadeMovimentacoes { get; set; }

        public DateTime? UltimaMovimentacao { get; set; }
        public decimal SaldoInicial { get; set; }
    }

    /// <summary>Rendimento consolidado de um mes especifico.</summary>
    public class RendimentoMensalDTO
    {
        public int Ano { get; set; }
        public int Mes { get; set; }

        /// <summary>Quanto rendeu (em R$) no mes.</summary>
        public decimal Rendimento { get; set; }
    }
}
