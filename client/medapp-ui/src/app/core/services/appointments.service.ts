// src/app/core/services/appointments.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AppointmentCU, AppointmentListItem, Doctor, Medicine,
  Patient, PagedResult, VisitType,
  ListParams
} from '../../models';

@Injectable({ providedIn: 'root' })
export class AppointmentsService {
  private http = inject(HttpClient);
  // Adjust only this prefix if your proxy is different:
  private base = '/api/medapp';

  // ---- Master list ----
  list(params: ListParams): Observable<PagedResult<AppointmentListItem>> {
    let p = new HttpParams();
    if (params.pageNumber) p = p.set('pageNumber', params.pageNumber);
    if (params.pageSize) p = p.set('pageSize', params.pageSize);
    if (params.search)   p = p.set('search', params.search);
    if (params.doctorId) p  = p.set('doctorId', params.doctorId);
    if (params.visitType) p = p.set('visitType', params.visitType);
    if (params.sort) p = p.set('sort', params.sort);
    if (params.dir) p = p.set('dir', params.dir);

    return this.http.get<PagedResult<AppointmentListItem>>(
      `${this.base}/appointments`, { params: p }
    );
  }

  // ---- CRUD ----
  get(id: number) {
    return this.http.get<any>(`${this.base}/appointments/${id}`);
  }

  create(dto: AppointmentCU) {
    return this.http.post<{ id: number }>(`${this.base}/appointments`, dto);
  }

  update(id: number, dto: AppointmentCU) {
    return this.http.put<void>(`${this.base}/appointments/${id}`, dto);
  }

  remove(id: number) {
    return this.http.delete<void>(`${this.base}/appointments/${id}`);
  }

  downloadPdf(id: number) {
    return this.http.get(`${this.base}/appointments/${id}/pdf`, { responseType: 'blob' });
  }

  sendEmail(id: number, toEmail: string): Observable<void> {
    const url = `${this.base}/appointments/${id}/email?toEmail=${encodeURIComponent(toEmail)}`;
    // backend expects POST with any body; empty object is fine
    return this.http.post<void>(url, {});
  }

  // ---- Lookups ----
  patients()  { return this.http.get<Patient[]>(`${this.base}/lookups/patient`); }
  doctors()   { return this.http.get<Doctor[]>(`${this.base}/lookups/doctor`); }
  medicines() { return this.http.get<Medicine[]>(`${this.base}/lookups/medicine`); }
}
