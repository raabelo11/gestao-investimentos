namespace GestaoFinanceira.Model
{
    /// <summary>
    /// Tipos de movimentacao registrados no historico de uma caixinha.
    /// </summary>
    public enum TipoMovimentacao
    {
        /// <summary>Dinheiro que voce coloca na caixinha (capital investido).</summary>
        Aporte = 1,

        /// <summary>Dinheiro que voce retira da caixinha.</summary>
        Resgate = 2,

        /// <summary>Rendimento lancado explicitamente para uma data.</summary>
        Rendimento = 3,

        /// <summary>
        /// Foto do saldo total da caixinha em uma data. O sistema deriva o
        /// rendimento implicito comparando com o saldo esperado ate entao.
        /// </summary>
        Saldo = 4
    }
}
