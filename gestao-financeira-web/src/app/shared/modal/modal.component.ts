import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-modal',
  template: `
    <div class="overlay" (click)="fechar.emit()">
      <div class="dialog card fade-in" (click)="$event.stopPropagation()">
        <header class="dialog-head">
          <h3>{{ titulo() }}</h3>
          <button class="btn-icon" (click)="fechar.emit()" aria-label="Fechar">
            <span class="msr">close</span>
          </button>
        </header>
        <div class="dialog-body">
          <ng-content />
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .overlay {
        position: fixed;
        inset: 0;
        background: rgba(4, 7, 14, 0.68);
        backdrop-filter: blur(4px);
        display: flex;
        align-items: flex-start;
        justify-content: center;
        padding: 6vh 18px 24px;
        z-index: 150;
        overflow-y: auto;
      }
      .dialog {
        width: 100%;
        max-width: 480px;
        padding: 0;
        overflow: hidden;
      }
      .dialog-head {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 18px 20px;
        border-bottom: 1px solid var(--border);
      }
      .dialog-head h3 { font-size: 17px; }
      .dialog-body { padding: 20px; }
    `,
  ],
})
export class ModalComponent {
  readonly titulo = input.required<string>();
  readonly fechar = output<void>();
}
