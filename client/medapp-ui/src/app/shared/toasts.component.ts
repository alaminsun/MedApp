import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription, timer } from 'rxjs';
import { Toast, ToastService } from './toast.service';

@Component({
  selector: 'app-toasts',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 1056;">
    <div *ngFor="let t of toasts" class="toast show mb-2 border-0 shadow-sm"
         [class.bg-success]="t.level==='success'"
         [class.bg-danger]="t.level==='error'"
         [class.bg-dark]="t.level==='info'"
         [class.text-white]="true">
      <div class="toast-body d-flex align-items-center">
        <span class="me-3">{{ t.text }}</span>
        <button class="btn-close btn-close-white ms-auto" (click)="dismiss(t)"></button>
      </div>
    </div>
  </div>
  `,
})
export class ToastsComponent implements OnInit, OnDestroy {
  toasts: Toast[] = [];
  private sub?: Subscription;

  constructor(private toast: ToastService) {}

  ngOnInit(): void {
    this.sub = this.toast.toasts$.subscribe(t => {
      this.toasts.push(t);
      const to = t.timeoutMs ?? 4000;
      timer(to).subscribe(() => this.dismiss(t));
    });
  }

  dismiss(t: Toast) {
    this.toasts = this.toasts.filter(x => x.id !== t.id);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
