import { Component, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, switchMap, tap } from 'rxjs/operators';
import { AppointmentsService } from '../../core/services/appointments.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AppointmentListItem, PagedResult, VisitType, Doctor } from '../../models';
import { EmailDialogComponent } from "./email-dialog.component";
import { ToastService } from '../../shared/toast.service';
import { Dialog } from '@angular/cdk/dialog';

@Component({
  standalone: true,
  selector: 'app-appointments-list',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DatePipe, RouterLink],
  templateUrl: './appointments-list.html',
  styleUrl: './appointments-list.css',
  providers: [DatePipe]
})
export class AppointmentsListComponent {
  private api = inject(AppointmentsService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  private toast = inject(ToastService);
  private dialog = inject(Dialog);
  // UI state
  loading = signal(false);
  result = signal<PagedResult<AppointmentListItem> | null>(null);

  // filters / controls
  searchCtrl = new FormControl<string>('', { nonNullable: true });
  doctorId: number | null = null;
  visitType: VisitType | '' = '';
  pageNumber = 1;
  pageSize = 10;
  sort: 'patient' | 'doctor' | 'appointmentDate' | 'visitType' = 'appointmentDate';
  dir: 'asc' | 'desc' = 'desc';

  doctors: Doctor[] = [];

  ngOnInit() {
    // hydrate from URL
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(q => {
        this.searchCtrl.setValue(q.get('q') ?? '', { emitEvent: false });
        this.doctorId = q.get('doctorId') ? Number(q.get('doctorId')) : null;
        this.visitType = (q.get('visitType') as VisitType | '') ?? '';
        this.pageNumber = q.get('pageNumber') ? Number(q.get('pageNumber')) : 1;
        this.pageSize = q.get('pageSize') ? Number(q.get('pageSize')) : 10;
        this.sort = (q.get('sort') as any) ?? 'appointmentDate';
        this.dir = (q.get('dir') as any) ?? 'desc';
        this.load();
      });

    // doctors for filter
    this.api.doctors().subscribe(d => (this.doctors = d));

    // debounced search
    this.searchCtrl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        tap(() => (this.pageNumber = 1)),
        tap(() => this.syncUrl()),
        switchMap(() => this.fetch())
      )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(r => this.result.set(r));
  }

  // derived
  get totalPages(): number {
    const r = this.result();
    return !r ? 1 : Math.max(1, Math.ceil(r.total / this.pageSize));
  }

  // actions
  load() { this.fetch().subscribe(r => this.result.set(r)); }

  private fetch() {
    this.loading.set(true);
    return this.api.list({
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      search: this.searchCtrl.value?.trim() || null,
      doctorId: this.doctorId,
      visitType: this.visitType || null,
      sort: this.sort,
      dir: this.dir
    }).pipe(tap(() => this.loading.set(false)));
  }

  applyFilters() {
    this.pageNumber = 1;
    this.syncUrl();
    this.load();
  }

  reset() {
    this.searchCtrl.setValue('');
    this.doctorId = null;
    this.visitType = '';
    this.pageNumber = 1;
    this.pageSize = 10;
    this.sort = 'appointmentDate';
    this.dir = 'desc';
    this.syncUrl();
    this.load();
  }

  goto(p: number) {
    if (p < 1 || p > this.totalPages) return;
    this.pageNumber = p;
    this.syncUrl();
    this.load();
  }

  sortBy(col: typeof this.sort) {
    if (this.sort === col) this.dir = this.dir === 'asc' ? 'desc' : 'asc';
    else { this.sort = col; this.dir = 'asc'; }
    this.pageNumber = 1;
    this.syncUrl();
    this.load();
  }

  remove(id: number) {
    if (!confirm('Delete this appointment?')) return;
    this.loading.set(true);
    this.api.remove(id).subscribe({
      next: () => {
        this.load();
        this.toast.success('Appointment deleted.');
      },
      error: () => this.loading.set(false)
    });
  }

  downloadPdf(id: number) {
    this.api.downloadPdf(id).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Prescription-${id}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    });
  }

  openEmail(id: number) {
    const dialogRef = this.dialog.open<string>(EmailDialogComponent, {
      width: '520px',
    });

    dialogRef.closed.subscribe(result => {
      if (result) {
        this.sendEmail(id, result);
      }
    });
  }

  sendEmail(id: number, to: string) {
    this.loading.set(true);
    this.api.sendEmail(id, to).subscribe({
      next: () => {
        this.loading.set(false);
        this.toast.success('Email sent with PDF attached.');
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error('Failed to send email. Please try again.');
        console.error(err);
      }
    });
  }

  private syncUrl() {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        q: this.searchCtrl.value || null,
        doctorId: this.doctorId ?? null,
        visitType: this.visitType || null,
        pageNumber: this.pageNumber,
        pageSize: this.pageSize,
        sort: this.sort,
        dir: this.dir
      },
      queryParamsHandling: 'merge'
    });
  }
}
