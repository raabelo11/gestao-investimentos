import { Component, inject, input, signal, computed, OnInit } from '@angular/core';
import { CurrencyPipe, DecimalPipe, DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { CaixinhaService } from '../../core/services/caixinha.service';
import { MovimentacaoService } from '../../core/services/movimentacao.service';
import { Caixinha } from '../../core/models/caixinha.model';
import {
  Movimentacao,
  MovimentacaoInput,
  TipoMovimentacao,
  TIPOS_MOVIMENTACAO,
} from '../../core/models/movimentacao.model';
import { ToastService } from '../../shared/toast/toast.service';
import { ModalComponent } from '../../shared/modal/modal.component';

@Component({
  selector: 'app-caixinha-detalhe',
  imports: [CurrencyPipe, DecimalPipe, DatePipe, RouterLink, ReactiveFormsModule, ModalComponent],
  templateUrl: './caixinha-detalhe.component.html',
  styleUrl: './caixinha-detalhe.component.scss',
})
export class CaixinhaDetalheComponent implements OnInit {
  readonly id = input.required<number>();

  private readonly caixinhaService = inject(CaixinhaService);
  private readonly movimentacaoService = inject(MovimentacaoService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  protected readonly tipos = TIPOS_MOVIMENTACAO;

  protected readonly caixinha = signal<Caixinha | null>(null);
  protected readonly movimentacoes = signal<Movimentacao[]>([]);
  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);

  protected readonly modalAberto = signal(false);
  protected readonly editandoId = signal<number | null>(null);
  protected readonly tipoSelecionado = signal<TipoMovimentacao>('Aporte');

  protected readonly metaTipo = computed(() =>
    this.tipos.find((t) => t.tipo === this.tipoSelecionado()),
  );

  protected readonly form = this.fb.nonNullable.group({
    tipo: ['Aporte' as TipoMovimentacao, Validators.required],
    valor: [null as number | null, [Validators.required, Validators.min(0.01)]],
    data: [this.hojeIso(), Validators.required],
    observacao: [''],
  });

  ngOnInit(): void {
    this.carregar();
  }

  protected carregar(): void {
    this.carregando.set(true);
    forkJoin({
      caixinha: this.caixinhaService.obterPorId(this.id()),
      movimentacoes: this.movimentacaoService.listar(this.id()),
    }).subscribe({
      next: ({ caixinha, movimentacoes }) => {
        this.caixinha.set(caixinha);
        this.movimentacoes.set(movimentacoes);
        this.carregando.set(false);
      },
      error: () => {
        this.carregando.set(false);
        this.toast.erro('Não foi possível carregar a caixinha.');
      },
    });
  }

  protected abrirCriacao(tipo: TipoMovimentacao): void {
    this.editandoId.set(null);
    this.tipoSelecionado.set(tipo);
    this.form.reset({ tipo, valor: null, data: this.hojeIso(), observacao: '' });
    this.modalAberto.set(true);
  }

  protected abrirEdicao(m: Movimentacao): void {
    this.editandoId.set(m.id);
    this.tipoSelecionado.set(m.tipo);
    this.form.reset({
      tipo: m.tipo,
      valor: m.valor,
      data: m.data.substring(0, 10),
      observacao: m.observacao ?? '',
    });
    this.modalAberto.set(true);
  }

  protected trocarTipo(tipo: TipoMovimentacao): void {
    this.tipoSelecionado.set(tipo);
    this.form.controls.tipo.setValue(tipo);
  }

  protected fecharModal(): void {
    this.modalAberto.set(false);
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const valores = this.form.getRawValue();
    const payload: MovimentacaoInput = {
      tipo: valores.tipo,
      valor: Number(valores.valor),
      data: new Date(valores.data + 'T00:00:00').toISOString(),
      observacao: valores.observacao?.trim() || null,
    };

    this.salvando.set(true);
    const editId = this.editandoId();
    const requisicao = editId
      ? this.movimentacaoService.atualizar(this.id(), editId, payload)
      : this.movimentacaoService.criar(this.id(), payload);

    requisicao.subscribe({
      next: () => {
        this.salvando.set(false);
        this.modalAberto.set(false);
        this.toast.sucesso(editId ? 'Movimentação atualizada.' : 'Movimentação registrada.');
        this.carregar();
      },
      error: () => {
        this.salvando.set(false);
        this.toast.erro('Não foi possível salvar a movimentação.');
      },
    });
  }

  protected excluir(m: Movimentacao): void {
    if (!confirm('Excluir esta movimentação?')) {
      return;
    }
    this.movimentacaoService.excluir(this.id(), m.id).subscribe({
      next: () => {
        this.toast.sucesso('Movimentação excluída.');
        this.carregar();
      },
      error: () => this.toast.erro('Falha ao excluir.'),
    });
  }

  protected metaDoTipo(tipo: TipoMovimentacao) {
    return this.tipos.find((t) => t.tipo === tipo)!;
  }

  private hojeIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}
