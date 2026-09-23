/**
 * Utilitario de formatacao monetaria em Real (BRL).
 * Centraliza o formato para uso fora do template quando necessario.
 */
const formatador = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

export function formatarMoeda(valor: number): string {
  return formatador.format(valor ?? 0);
}
