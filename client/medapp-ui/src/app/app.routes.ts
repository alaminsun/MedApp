// src/app/app.routes.ts
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'appointments', pathMatch: 'full' },
  { path: 'appointments', loadComponent: () => import('./pages/appointments-list/appointments-list').then(m => m.AppointmentsListComponent) },
  { path: 'appointments/new', loadComponent: () => import('./pages/appointment-form/appointment-form').then(m => m.AppointmentFormComponent) },
  { path: 'appointments/:id', loadComponent: () => import('./pages/appointment-form/appointment-form').then(m => m.AppointmentFormComponent) },
];
