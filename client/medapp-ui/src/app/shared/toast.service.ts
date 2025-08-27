import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export type ToastLevel = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  level: ToastLevel;
  text: string;
  timeoutMs?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private seq = 0;
  private _toasts$ = new Subject<Toast>();
  toasts$ = this._toasts$.asObservable();

  show(level: ToastLevel, text: string, timeoutMs = 4000) {
    this._toasts$.next({ id: ++this.seq, level, text, timeoutMs });
  }

  success(text: string, timeoutMs?: number) { this.show('success', text, timeoutMs); }
  error(text: string, timeoutMs?: number)   { this.show('error', text, timeoutMs); }
  info(text: string, timeoutMs?: number)    { this.show('info', text, timeoutMs); }
}
