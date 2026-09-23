export interface RendimentoMensal {
  ano: number;
  mes: number;
  rendimento: number;
}

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
  saldoInicial?: number | null;
  capitalInvestido: number;
  totalAportado: number;
  totalResgatado: number;
  rendimentoAcumulado: number;
  /** Media, em R$, do quanto rendeu por mes (meses com rendimento). */
  rentabilidadeMediaMensal: number;
  historicoMensal: RendimentoMensal[];
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
  saldoInicial?: number | null;
}
