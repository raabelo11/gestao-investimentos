import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

import { DashboardService } from '../../core/services/dashboard.service';
import { Dashboard } from '../../core/models/dashboard.model';
import { ToastService } from '../../shared/toast/toast.service';
import { EvolucaoChartComponent } from '../../shared/chart/evolucao-chart.component';

@Component({
  selector: 'app-dashboard',
  imports: [CurrencyPipe, DecimalPipe, RouterLink, EvolucaoChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly toast = inject(ToastService);

  protected readonly dados = signal<Dashboard | null>(null);
  protected readonly carregando = signal(true);
  protected readonly erro = signal(false);

  protected readonly temCaixinhas = computed(() => (this.dados()?.quantidadeCaixinhas ?? 0) > 0);

  ngOnInit(): void {
    this.carregar();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erro.set(false);
    this.dashboardService.obter().subscribe({
      next: (d) => {
        this.dados.set(d);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set(true);
        this.carregando.set(false);
        this.toast.erro('Não foi possível carregar o dashboard. A API está rodando?');
      },
    });
  }

  protected corBarra(saldo: number, total: number): number {
    if (total <= 0) return 0;
    return Math.min(100, (saldo / total) * 100);
  }
}
