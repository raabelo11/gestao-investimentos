export interface Caixinha {
  id: number;
  nome: string;
  descricao?: string | null;
  cor: string;
  icone: string;
  meta?: number | null;
  arquivada: boolean;
  criadaEm: string;
  saldoAtual: number;
  capitalInvestido: number;
  totalAportado: number;
  totalResgatado: number;
  rendimentoAcumulado: number;
  rentabilidadePercentual: number;
  percentualMeta?: number | null;
  quantidadeMovimentacoes: number;
  ultimaMovimentacao?: string | null;
}

export interface CaixinhaInput {
  nome: string;
  descricao?: string | null;
  cor: string;
  icone: string;
  meta?: number | null;
}
