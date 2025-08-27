// src/app/models.ts
export interface Patient { id: number; name: string; age: number; }
export interface Doctor { id: number; name: string; specialty: string; }
export interface Medicine { id: number; name: string; dosage?: string; manufacturer?: string; }

export enum VisitType { First = 'First', FollowUp = 'FollowUp' }
//export type VisitType = 'First' | 'FollowUp';

export interface AppointmentListItem {
  id: number;
  patient: string;
  doctor: string;
  date: string;   // ISO
  visitType: VisitType;
  diagnosis: string | null;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  pageNumber: number;
  pageSize: number;
}

export interface ListParams {
  pageNumber?: number;
  pageSize?: number;
  search?: string | null;
  doctorId?: number | null;
  visitType?: VisitType | null;
  sort?: 'patient' | 'doctor' | 'appointmentDate' | 'visitType';
  dir?: 'asc' | 'desc';
}

export interface PrescriptionRow {
  id?: number;
  medicineId: number | null;
  dosage: string;
  startDate: string;          // yyyy-MM-dd
  endDate: string;            // yyyy-MM-dd
  notes?: string;
}

export interface AppointmentCU {
  patientId: number | null;
  doctorId: number | null;
  appointmentDate: string;    // yyyy-MM-dd
  visitType: VisitType;
  notes?: string;
  diagnosis?: string;
  prescriptions: PrescriptionRow[];
}


