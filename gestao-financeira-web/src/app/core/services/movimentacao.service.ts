import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Movimentacao, MovimentacaoInput } from '../models/movimentacao.model';

@Injectable({ providedIn: 'root' })
export class MovimentacaoService {
  private readonly http = inject(HttpClient);

  private baseUrl(caixinhaId: number): string {
    return `${environment.apiBaseUrl}/caixinhas/${caixinhaId}/movimentacoes`;
  }

  listar(caixinhaId: number): Observable<Movimentacao[]> {
    return this.http.get<Movimentacao[]>(this.baseUrl(caixinhaId));
  }

  criar(caixinhaId: number, input: MovimentacaoInput): Observable<Movimentacao> {
    return this.http.post<Movimentacao>(this.baseUrl(caixinhaId), input);
  }

  atualizar(caixinhaId: number, id: number, input: MovimentacaoInput): Observable<Movimentacao> {
    return this.http.put<Movimentacao>(`${this.baseUrl(caixinhaId)}/${id}`, input);
  }

  excluir(caixinhaId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl(caixinhaId)}/${id}`);
  }
}
