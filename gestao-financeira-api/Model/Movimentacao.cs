using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceira.Model
{
    /// <summary>
    /// Um evento no historico de uma caixinha. Dependendo do tipo, o valor
    /// tem significados diferentes (ver <see cref="TipoMovimentacao"/>).
    /// </summary>
    public class Movimentacao
    {
        public int Id { get; set; }

        public int CaixinhaId { get; set; }

        public Caixinha? Caixinha { get; set; }

        public TipoMovimentacao Tipo { get; set; }

        /// <summary>
        /// Para Aporte/Resgate/Rendimento: o valor do evento.
        /// Para Saldo: o saldo total informado naquela data.
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>Data de competencia informada pelo usuario (dia do evento).</summary>
        public DateTime Data { get; set; }

        [MaxLength(280)]
        public string? Observacao { get; set; }

        public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
    }
}
