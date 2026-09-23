import { Injectable, signal } from '@angular/core';

export type ToastTipo = 'sucesso' | 'erro' | 'info';

export interface Toast {
  id: number;
  tipo: ToastTipo;
  mensagem: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();
  private contador = 0;

  sucesso(mensagem: string): void {
    this.adicionar('sucesso', mensagem);
  }

  erro(mensagem: string): void {
    this.adicionar('erro', mensagem);
  }

  info(mensagem: string): void {
    this.adicionar('info', mensagem);
  }

  remover(id: number): void {
    this._toasts.update((lista) => lista.filter((t) => t.id !== id));
  }

  private adicionar(tipo: ToastTipo, mensagem: string): void {
    const id = ++this.contador;
    this._toasts.update((lista) => [...lista, { id, tipo, mensagem }]);
    setTimeout(() => this.remover(id), 4200);
  }
}
