import { Component, inject, signal, OnInit } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { CaixinhaService } from '../../core/services/caixinha.service';
import { Caixinha, CaixinhaInput } from '../../core/models/caixinha.model';
import { ToastService } from '../../shared/toast/toast.service';
import { ModalComponent } from '../../shared/modal/modal.component';

const ICONES = [
  'savings',
  'account_balance',
  'trending_up',
  'paid',
  'diamond',
  'home',
  'directions_car',
  'flight_takeoff',
  'school',
  'health_and_safety',
  'beach_access',
  'redeem',
];

const CORES = [
  '#6366f1',
  '#22c55e',
  '#3b82f6',
  '#f59e0b',
  '#ef4444',
  '#8b5cf6',
  '#ec4899',
  '#14b8a6',
];

@Component({
  selector: 'app-caixinhas',
  imports: [CurrencyPipe, RouterLink, ReactiveFormsModule, ModalComponent],
  templateUrl: './caixinhas.component.html',
  styleUrl: './caixinhas.component.scss',
})
export class CaixinhasComponent implements OnInit {
  private readonly caixinhaService = inject(CaixinhaService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  protected readonly icones = ICONES;
  protected readonly cores = CORES;

  protected readonly caixinhas = signal<Caixinha[]>([]);
  protected readonly carregando = signal(true);
  protected readonly mostrarArquivadas = signal(false);
  protected readonly salvando = signal(false);

  protected readonly modalAberto = signal(false);
  protected readonly editandoId = signal<number | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.maxLength(80)]],
    descricao: [''],
    saldoInicial: [null as number | null, [Validators.min(0)]],
    cor: [CORES[0]],
    icone: [ICONES[0]],
    meta: [null as number | null],
  });

  ngOnInit(): void {
    this.carregar();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.caixinhaService.listar(this.mostrarArquivadas()).subscribe({
      next: (lista) => {
        this.caixinhas.set(lista);
        this.carregando.set(false);
      },
      error: () => {
        this.carregando.set(false);
        this.toast.erro('Falha ao carregar caixinhas.');
      },
    });
  }

  protected alternarArquivadas(): void {
    this.mostrarArquivadas.update((v) => !v);
    this.carregar();
  }

  protected abrirCriacao(): void {
    this.editandoId.set(null);
    this.form.reset({ nome: '', descricao: '', cor: CORES[0], icone: ICONES[0], meta: null });
    this.modalAberto.set(true);
  }

  protected abrirEdicao(c: Caixinha, evento: Event): void {
    evento.preventDefault();
    evento.stopPropagation();
    this.editandoId.set(c.id);
    this.form.reset({
      nome: c.nome,
      descricao: c.descricao ?? '',
      cor: c.cor,
      icone: c.icone,
      meta: c.meta ?? null,
    });
    this.modalAberto.set(true);
  }

  protected fecharModal(): void {
    this.modalAberto.set(false);
  }

  protected selecionarCor(cor: string): void {
    this.form.controls.cor.setValue(cor);
  }

  protected selecionarIcone(icone: string): void {
    this.form.controls.icone.setValue(icone);
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const valores = this.form.getRawValue();
    const payload: CaixinhaInput = {
      nome: valores.nome.trim(),
      descricao: valores.descricao?.trim() || null,
      cor: valores.cor,
      icone: valores.icone,
      meta: valores.meta ? Number(valores.meta) : null,
      saldoInicial: valores.saldoInicial ? Number(valores.saldoInicial) : null,
    };

    this.salvando.set(true);
    const id = this.editandoId();
    const requisicao = id
      ? this.caixinhaService.atualizar(id, payload)
      : this.caixinhaService.criar(payload);

    requisicao.subscribe({
      next: () => {
        this.salvando.set(false);
        this.modalAberto.set(false);
        this.toast.sucesso(id ? 'Caixinha atualizada.' : 'Caixinha criada.');
        this.carregar();
      },
      error: () => {
        this.salvando.set(false);
        this.toast.erro('Não foi possível salvar a caixinha.');
      },
    });
  }

  protected arquivar(c: Caixinha, evento: Event): void {
    evento.preventDefault();
    evento.stopPropagation();
    this.caixinhaService.alternarArquivamento(c.id, !c.arquivada).subscribe({
      next: () => {
        this.toast.sucesso(c.arquivada ? 'Caixinha reativada.' : 'Caixinha arquivada.');
        this.carregar();
      },
      error: () => this.toast.erro('Falha ao arquivar.'),
    });
  }

  protected excluir(c: Caixinha, evento: Event): void {
    evento.preventDefault();
    evento.stopPropagation();
    if (!confirm(`Excluir a caixinha "${c.nome}" e todo o seu histórico? Esta ação é irreversível.`)) {
      return;
    }
    this.caixinhaService.excluir(c.id).subscribe({
      next: () => {
        this.toast.sucesso('Caixinha excluída.');
        this.carregar();
      },
      error: () => this.toast.erro('Falha ao excluir.'),
    });
  }
}
