import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
    title: 'Dashboard | Gestão Financeira',
  },
  {
    path: 'caixinhas',
    loadComponent: () =>
      import('./features/caixinhas/caixinhas.component').then((m) => m.CaixinhasComponent),
    title: 'Caixinhas | Gestão Financeira',
  },
  {
    path: 'caixinhas/:id',
    loadComponent: () =>
      import('./features/caixinha-detalhe/caixinha-detalhe.component').then(
        (m) => m.CaixinhaDetalheComponent,
      ),
    title: 'Caixinha | Gestão Financeira',
  },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', redirectTo: 'dashboard' },
];
