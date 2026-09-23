using System.ComponentModel.DataAnnotations;
using GestaoFinanceira.Model;

namespace GestaoFinanceira.DTOs
{
    /// <summary>Payload para registrar ou atualizar uma movimentacao.</summary>
    public class MovimentacaoInputDTO
    {
        [Required]
        public TipoMovimentacao Tipo { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A data e obrigatoria.")]
        public DateTime Data { get; set; }

        [MaxLength(280)]
        public string? Observacao { get; set; }
    }

    /// <summary>Representacao de leitura de uma movimentacao.</summary>
    public class MovimentacaoDTO
    {
        public int Id { get; set; }
        public int CaixinhaId { get; set; }
        public TipoMovimentacao Tipo { get; set; }
        public string TipoDescricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Observacao { get; set; }
        public DateTime CriadaEm { get; set; }

        /// <summary>
        /// Para movimentacoes do tipo Saldo: rendimento derivado ao reconciliar
        /// o saldo informado com o saldo esperado ate aquela data.
        /// </summary>
        public decimal? RendimentoDerivado { get; set; }
    }
}
