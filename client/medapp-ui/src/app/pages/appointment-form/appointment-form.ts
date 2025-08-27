import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AppointmentsService } from '../../core/services/appointments.service';
import { AppointmentCU, Doctor, Medicine, Patient, PrescriptionRow, VisitType } from '../../models';
import { forkJoin } from 'rxjs';
import { ToastService } from '../../shared/toast.service';

type RxFG = FormGroup<{
  id: FormControl<number | null>;
  medicineId: FormControl<number | null>;
  dosage: FormControl<string>;
  startDate: FormControl<string>;
  endDate: FormControl<string>;
  notes: FormControl<string>;
}>;

@Component({
  selector: 'app-appointment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './appointment-form.html',
  styleUrl: './appointment-form.css'
})
export class AppointmentFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(AppointmentsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  id: number | null = null;
  patients: Patient[] = [];
  doctors: Doctor[] = [];
  medicines: Medicine[] = [];
  saving = false;

  visitTypes = [VisitType.First, VisitType.FollowUp];
// keep this so the template can write VisitTypeEnum.First
readonly VisitTypeEnum = VisitType;

  form: FormGroup = this.fb.group({
    patientId: [null, Validators.required],
    doctorId: [null, Validators.required],
    appointmentDate: [this.todayIso(), Validators.required],
    visitType: [VisitType.First, Validators.required],
    notes: [''],
    diagnosis: [''],
    prescriptions: this.fb.array<RxFG>([])
  });

  // --- row-level validator: ensures startDate <= endDate
  private dateOrderValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
    const start = group.get('startDate')?.value as string | null;
    const end = group.get('endDate')?.value as string | null;
    if (!start || !end) return null;                  // "required" handled by controls
    return start <= end ? null : { dateOrder: true }; // compare yyyy-MM-dd safely
  };

  // helper for template CSS
  isInvalid(ctrl: AbstractControl | null): boolean {
    return !!ctrl && ctrl.invalid && (ctrl.touched || ctrl.dirty);
  }

  get prescriptions(): FormArray<RxFG> {
    return this.form.get('prescriptions') as FormArray<RxFG>;
  }

  ngOnInit() {
    this.id = Number(this.route.snapshot.paramMap.get('id')) || null;
    forkJoin({
      patients: this.api.patients(),
      doctors: this.api.doctors(),
      meds: this.api.medicines()
    }).subscribe(({ patients, doctors, meds }) => {
      this.patients = patients; this.doctors = doctors; this.medicines = meds;

      if (this.id) {
        this.api.get(this.id).subscribe(a => {
          this.form.patchValue({
            patientId: a.patientId,
            doctorId: a.doctorId,
            appointmentDate: a.appointmentDate?.substring(0, 10),
            visitType: a.visitType,
            notes: a.notes || '',
            diagnosis: a.diagnosis || ''
          });
          (a.prescriptionDetails || []).forEach((p: any) => {
            this.prescriptions.push(this.newRx({
              id: p.id,
              medicineId: p.medicineId,
              dosage: p.dosage,
              startDate: p.startDate?.substring(0, 10),
              endDate: p.endDate?.substring(0, 10),
              notes: p.notes || ''
            }));
          });
          if (this.prescriptions.length === 0) this.addRx();
        });
      } else {
        // one empty row to start
        this.addRx();
      }
    });
  }

  private newRx(row?: Partial<PrescriptionRow>): RxFG {
    // attach the row-level validator here
    return this.fb.group({
      id: this.fb.control<number | null>(row?.id ?? null),
      medicineId: this.fb.control<number | null>(row?.medicineId ?? null, Validators.required),
      dosage: this.fb.control<string>(row?.dosage ?? '', Validators.required),
      startDate: this.fb.control<string>(row?.startDate ?? this.todayIso(), Validators.required),
      endDate: this.fb.control<string>(row?.endDate ?? this.todayIso(), Validators.required),
      notes: this.fb.control<string>(row?.notes ?? '')
    }, { validators: this.dateOrderValidator }) as RxFG;
  }

  addRx() { this.prescriptions.push(this.newRx()); }
  removeRx(i: number) { this.prescriptions.removeAt(i); }

  save() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    const dto: AppointmentCU = this.form.value as AppointmentCU;
    if (this.id) {
      this.api.update(this.id, dto).subscribe(() => {
        this.router.navigate(['/appointments']);
        this.toast.success('Appointment updated.');
      });
    } else {
      this.api.create(dto).subscribe(() => {
        this.router.navigate(['/appointments']);
        this.toast.success('Appointment created.');
      });
    }
  }

  cancel() { this.router.navigate(['/appointments']); }

  private todayIso() {
    return new Date().toISOString().substring(0, 10);
  }

  /** Returns true when rx.endDate < rx.startDate (both present). */
isDateOrderInvalid(row: import('@angular/forms').AbstractControl | null): boolean {
  if (!row) return false;
  const s = row.get('startDate')?.value as string | undefined;
  const e = row.get('endDate')?.value as string | undefined;
  return !!(s && e && e < s);
}

/** Should the End Date control render as invalid? */
isEndDateInvalid(row: import('@angular/forms').AbstractControl | null): boolean {
  const end = row?.get('endDate');
  return !!end && end.touched && (end.hasError('required') || this.isDateOrderInvalid(row));
}
}