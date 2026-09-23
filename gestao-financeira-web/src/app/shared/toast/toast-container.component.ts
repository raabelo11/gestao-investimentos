import { Component, inject } from '@angular/core';

import { ToastService, ToastTipo } from './toast.service';

@Component({
  selector: 'app-toast-container',
  template: `
    <div class="toast-wrap">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast" [class]="'toast-' + toast.tipo" (click)="toastService.remover(toast.id)">
          <span class="msr">{{ icone(toast.tipo) }}</span>
          <span class="toast-msg">{{ toast.mensagem }}</span>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .toast-wrap {
        position: fixed;
        top: 20px;
        right: 20px;
        display: flex;
        flex-direction: column;
        gap: 10px;
        z-index: 200;
        max-width: 360px;
      }
      .toast {
        display: flex;
        align-items: center;
        gap: 10px;
        padding: 13px 16px;
        border-radius: 12px;
        background: var(--surface-2);
        border: 1px solid var(--border-strong);
        box-shadow: var(--shadow-lg);
        color: var(--text);
        font-size: 14px;
        font-weight: 500;
        cursor: pointer;
        animation: toastIn 0.28s ease both;
      }
      .toast .msr { font-size: 20px; flex-shrink: 0; }
      .toast-msg { line-height: 1.35; }
      .toast-sucesso { border-left: 3px solid var(--success); }
      .toast-sucesso .msr { color: var(--success); }
      .toast-erro { border-left: 3px solid var(--danger); }
      .toast-erro .msr { color: var(--danger); }
      .toast-info { border-left: 3px solid var(--info); }
      .toast-info .msr { color: var(--info); }
      @keyframes toastIn {
        from { opacity: 0; transform: translateX(24px); }
        to { opacity: 1; transform: translateX(0); }
      }
    `,
  ],
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);

  protected icone(tipo: ToastTipo): string {
    switch (tipo) {
      case 'sucesso':
        return 'check_circle';
      case 'erro':
        return 'error';
      default:
        return 'info';
    }
  }
}
