import { Caixinha } from './caixinha.model';

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
  rentabilidadePercentualGeral: number;
  totalAportado: number;
  totalResgatado: number;
  quantidadeCaixinhas: number;
  caixinhas: Caixinha[];
  evolucaoPatrimonio: PontoEvolucao[];
}
