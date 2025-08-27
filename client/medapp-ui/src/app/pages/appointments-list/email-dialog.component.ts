import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule, DialogRef } from '@angular/cdk/dialog';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

@Component({
  selector: 'app-email-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DialogModule],
  template: `
    <form class="box" [formGroup]="form" (ngSubmit)="submit()" role="dialog" aria-labelledby="dlgTitle">
      <h3 id="dlgTitle">Send Prescription PDF</h3>

      <label for="email" class="lbl">Email address</label>
      <input id="email" type="email" formControlName="email" placeholder="patient@example.com" autofocus />
      <div class="err" *ngIf="email?.touched && email?.invalid">
        <span *ngIf="email?.errors?.['required']">Email is required.</span>
        <span *ngIf="email?.errors?.['email']">Please enter a valid email.</span>
      </div>

      <p class="hint">We’ll email the PDF report to this address.</p>

      <div class="actions">
        <button type="button" class="btn secondary" (click)="cancel()">Cancel</button>
        <button type="submit" class="btn primary" [disabled]="form.invalid || busy">
          {{ busy ? 'Sending…' : 'Send' }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .box{width:min(92vw,520px);background:#fff;border-radius:12px;
         padding:16px;box-shadow:0 20px 60px rgba(0,0,0,.25);}
    h3{margin:0 0 12px;font:600 16px/1.4 system-ui,Segoe UI,Roboto,Arial}
    .lbl{display:block;margin:10px 0 6px;font-weight:600}
    input{width:100%;padding:10px 12px;border:1px solid #d6d6db;border-radius:8px;outline:none}
    input:focus{border-color:#6e56cf;box-shadow:0 0 0 3px rgba(110,86,207,.15)}
    .hint{margin:8px 0 16px;color:#6b7280;font-size:12px}
    .actions{display:flex;justify-content:flex-end;gap:8px}
    .btn{padding:8px 14px;border-radius:8px;border:1px solid transparent;font-weight:600;cursor:pointer}
    .btn.secondary{background:#fff;border-color:#d6d6db}
    .btn.secondary:hover{background:#f3f4f6}
    .btn.primary{background:#4f46e5;color:#fff}
    .btn.primary:hover{background:#4338ca}
    .btn:disabled{opacity:.6;cursor:default}
    .err{color:#b91c1c;font-size:12px;margin-top:6px}
  `]
})
export class EmailDialogComponent {
  private fb = inject(FormBuilder);               // ✅ safe in field initializer
  private ref = inject(DialogRef<string>);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  busy = false;

  get email() { return this.form.get('email'); }

  submit() {
    if (this.form.invalid || this.busy) return;
    this.ref.close(this.form.value.email!.trim()); // return the email to opener
  }

  cancel() {
    this.ref.close();
  }
}
