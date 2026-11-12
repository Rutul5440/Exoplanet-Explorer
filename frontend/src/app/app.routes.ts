import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/dashboard/observatory-dashboard').then((m) => m.ObservatoryDashboard)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
