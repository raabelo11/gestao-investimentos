export type TipoMovimentacao = 'Aporte' | 'Resgate' | 'Rendimento' | 'Saldo';

export interface Movimentacao {
  id: number;
  caixinhaId: number;
  tipo: TipoMovimentacao;
  tipoDescricao: string;
  valor: number;
  data: string;
  observacao?: string | null;
  criadaEm: string;
  rendimentoDerivado?: number | null;
}

export interface MovimentacaoInput {
  tipo: TipoMovimentacao;
  valor: number;
  data: string;
  observacao?: string | null;
}

export interface TipoMovimentacaoMeta {
  tipo: TipoMovimentacao;
  rotulo: string;
  descricao: string;
  icone: string;
  cor: string;
}

export const TIPOS_MOVIMENTACAO: TipoMovimentacaoMeta[] = [
  {
    tipo: 'Aporte',
    rotulo: 'Aporte',
    descricao: 'Dinheiro que você adicionou à caixinha',
    icone: 'add_circle',
    cor: '#22c55e',
  },
  {
    tipo: 'Resgate',
    rotulo: 'Resgate',
    descricao: 'Dinheiro que você retirou da caixinha',
    icone: 'remove_circle',
    cor: '#f97316',
  },
  {
    tipo: 'Saldo',
    rotulo: 'Saldo do dia',
    descricao: 'Informe o saldo total atual — o sistema calcula quanto rendeu',
    icone: 'account_balance',
    cor: '#8b5cf6',
  },
];
