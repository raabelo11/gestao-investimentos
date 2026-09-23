namespace GestaoFinanceira.DTOs
{
    /// <summary>Visao consolidada de todo o patrimonio, para o dashboard.</summary>
    public class DashboardDTO
    {
        public decimal PatrimonioTotal { get; set; }
        public decimal CapitalInvestidoTotal { get; set; }
        public decimal RendimentoAcumuladoTotal { get; set; }
        public decimal RentabilidadePercentualGeral { get; set; }
        public decimal TotalAportado { get; set; }
        public decimal TotalResgatado { get; set; }
        public int QuantidadeCaixinhas { get; set; }

        /// <summary>Caixinhas ativas com seus indicadores, ordenadas por saldo.</summary>
        public List<CaixinhaResumoDTO> Caixinhas { get; set; } = new();

        /// <summary>Serie temporal do patrimonio para grafico de evolucao.</summary>
        public List<PontoEvolucaoDTO> EvolucaoPatrimonio { get; set; } = new();
    }

    /// <summary>Ponto da serie de evolucao do patrimonio.</summary>
    public class PontoEvolucaoDTO
    {
        public DateTime Data { get; set; }
        public decimal Saldo { get; set; }
        public decimal CapitalInvestido { get; set; }
        public decimal RendimentoAcumulado { get; set; }
    }
}
