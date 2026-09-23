import { Caixinha, RendimentoMensal } from './caixinha.model';

export interface PontoEvolucao {
  data: string;
  saldo: number;
  capitalInvestido: number;
  rendimentoAcumulado: number;
}

export interface Dashboard {
  patrimonioTotal: number;
  capitalInvestidoTotal: number;
  rendimentoAcumuladoTotal: number;
  /** Media, em R$, do quanto rendeu por mes somando todas as caixinhas. */
  rentabilidadeMediaMensalGeral: number;
  totalAportado: number;
  totalResgatado: number;
  quantidadeCaixinhas: number;
  caixinhas: Caixinha[];
  evolucaoPatrimonio: PontoEvolucao[];
  historicoMensal: RendimentoMensal[];
}
