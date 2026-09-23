import { Component, computed, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';

import { PontoEvolucao } from '../../core/models/dashboard.model';

interface Serie {
  saldoPath: string;
  areaPath: string;
  capitalPath: string;
  pontos: { x: number; y: number; ponto: PontoEvolucao }[];
  gridY: { y: number; valor: number }[];
  labelsX: { x: number; texto: string }[];
}

/**
 * Grafico de evolucao do patrimonio desenhado em SVG puro (sem dependencias).
 * Mostra a linha de saldo (com area) e a linha de capital investido.
 */
@Component({
  selector: 'app-evolucao-chart',
  imports: [DecimalPipe],
  template: `
    @if (serie(); as s) {
      <div class="chart">
        <svg [attr.viewBox]="'0 0 ' + w + ' ' + h" preserveAspectRatio="none" class="svg">
          <defs>
            <linearGradient id="areaGrad" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stop-color="var(--primary)" stop-opacity="0.35" />
              <stop offset="100%" stop-color="var(--primary)" stop-opacity="0" />
            </linearGradient>
          </defs>

          @for (g of s.gridY; track g.y) {
            <line class="grid" [attr.x1]="padL" [attr.x2]="w - padR" [attr.y1]="g.y" [attr.y2]="g.y" />
          }

          <path class="area" [attr.d]="s.areaPath" />
          <path class="line-capital" [attr.d]="s.capitalPath" />
          <path class="line-saldo" [attr.d]="s.saldoPath" />

          @for (p of s.pontos; track p.x) {
            <circle class="dot" [attr.cx]="p.x" [attr.cy]="p.y" r="3" />
          }
        </svg>

        <div class="y-labels">
          @for (g of s.gridY; track g.valor) {
            <span [style.top.px]="g.y - 8">{{ g.valor | number: '1.0-0' }}</span>
          }
        </div>

        <div class="legend">
          <span class="lg"><i class="sw saldo"></i>Patrimônio</span>
          <span class="lg"><i class="sw capital"></i>Capital investido</span>
        </div>
      </div>
    } @else {
      <div class="empty-state">
        <span class="msr">show_chart</span>
        <p>Registre movimentações para ver a evolução do patrimônio.</p>
      </div>
    }
  `,
  styles: [
    `
      .chart { position: relative; width: 100%; }
      .svg { width: 100%; height: 260px; display: block; overflow: visible; }
      .grid { stroke: var(--border); stroke-width: 1; stroke-dasharray: 4 6; }
      .area { fill: url(#areaGrad); stroke: none; }
      .line-saldo {
        fill: none;
        stroke: var(--primary);
        stroke-width: 2.5;
        stroke-linejoin: round;
        stroke-linecap: round;
      }
      .line-capital {
        fill: none;
        stroke: var(--text-faint);
        stroke-width: 1.6;
        stroke-dasharray: 5 5;
        stroke-linejoin: round;
      }
      .dot { fill: var(--primary); stroke: var(--bg-app); stroke-width: 2; }
      .y-labels { position: absolute; inset: 0; pointer-events: none; }
      .y-labels span {
        position: absolute;
        left: 0;
        font-size: 11px;
        color: var(--text-faint);
      }
      .legend {
        display: flex;
        gap: 18px;
        margin-top: 12px;
        padding-left: 8px;
      }
      .lg { display: inline-flex; align-items: center; gap: 7px; font-size: 12.5px; color: var(--text-muted); }
      .sw { width: 16px; height: 3px; border-radius: 2px; display: inline-block; }
      .sw.saldo { background: var(--primary); }
      .sw.capital { background: var(--text-faint); }
    `,
  ],
})
export class EvolucaoChartComponent {
  readonly dados = input.required<PontoEvolucao[]>();

  protected readonly w = 720;
  protected readonly h = 260;
  protected readonly padL = 44;
  protected readonly padR = 12;
  protected readonly padT = 16;
  protected readonly padB = 28;

  protected readonly serie = computed<Serie | null>(() => {
    const dados = this.dados();
    if (!dados || dados.length < 2) {
      return null;
    }

    const valores = dados.flatMap((d) => [d.saldo, d.capitalInvestido]);
    const max = Math.max(...valores, 1);
    const min = Math.min(...valores, 0);
    const span = max - min || 1;

    const innerW = this.w - this.padL - this.padR;
    const innerH = this.h - this.padT - this.padB;

    const x = (i: number) => this.padL + (innerW * i) / (dados.length - 1);
    const y = (v: number) => this.padT + innerH - (innerH * (v - min)) / span;

    const pontos = dados.map((ponto, i) => ({ x: x(i), y: y(ponto.saldo), ponto }));

    const saldoPath = pontos.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ');
    const capitalPath = dados
      .map((d, i) => `${i === 0 ? 'M' : 'L'} ${x(i)} ${y(d.capitalInvestido)}`)
      .join(' ');
    const areaPath =
      `${saldoPath} L ${pontos[pontos.length - 1].x} ${this.padT + innerH}` +
      ` L ${pontos[0].x} ${this.padT + innerH} Z`;

    const gridLinhas = 4;
    const gridY = Array.from({ length: gridLinhas + 1 }, (_, i) => {
      const valor = max - (span * i) / gridLinhas;
      return { y: this.padT + (innerH * i) / gridLinhas, valor };
    });

    const passo = Math.max(1, Math.ceil(dados.length / 6));
    const labelsX = dados
      .map((d, i) => ({ x: x(i), texto: this.formatarData(d.data), i }))
      .filter((_, i) => i % passo === 0);

    return { saldoPath, areaPath, capitalPath, pontos, gridY, labelsX };
  });

  private formatarData(iso: string): string {
    const d = new Date(iso);
    return d.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });
  }
}
