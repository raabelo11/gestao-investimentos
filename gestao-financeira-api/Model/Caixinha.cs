using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceira.Model
{
    /// <summary>
    /// Representa uma "caixinha" de investimento: um agrupamento independente
    /// de capital com seu proprio saldo, aportes, resgates e rendimentos.
    /// </summary>
    public class Caixinha
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(280)]
        public string? Descricao { get; set; }

        /// <summary>Cor em hex (ex.: #4F46E5) para exibicao no front.</summary>
        [MaxLength(9)]
        public string Cor { get; set; } = "#4F46E5";

        /// <summary>Nome do icone usado no front (ex.: savings, trending_up).</summary>
        [MaxLength(40)]
        public string Icone { get; set; } = "savings";

        /// <summary>Meta opcional de valor a ser atingido pela caixinha.</summary>
        public decimal? Meta { get; set; }

        public bool Arquivada { get; set; }

        public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
        public decimal SaldoInicial { get; set; } = 0m;

        public List<Movimentacao> Movimentacoes { get; set; } = new();
    }
}
