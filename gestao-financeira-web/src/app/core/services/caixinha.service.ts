import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Caixinha, CaixinhaInput } from '../models/caixinha.model';

@Injectable({ providedIn: 'root' })
export class CaixinhaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/caixinhas`;

  listar(incluirArquivadas = false): Observable<Caixinha[]> {
    const params = new HttpParams().set('incluirArquivadas', incluirArquivadas);
    return this.http.get<Caixinha[]>(this.baseUrl, { params });
  }

  obterPorId(id: number): Observable<Caixinha> {
    return this.http.get<Caixinha>(`${this.baseUrl}/${id}`);
  }

  criar(input: CaixinhaInput): Observable<Caixinha> {
    return this.http.post<Caixinha>(this.baseUrl, input);
  }

  atualizar(id: number, input: CaixinhaInput): Observable<Caixinha> {
    return this.http.put<Caixinha>(`${this.baseUrl}/${id}`, input);
  }

  alternarArquivamento(id: number, arquivar: boolean): Observable<Caixinha> {
    const params = new HttpParams().set('arquivar', arquivar);
    return this.http.patch<Caixinha>(`${this.baseUrl}/${id}/arquivamento`, null, { params });
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
