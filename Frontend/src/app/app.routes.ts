import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { FileListComponent } from './features/files/file-list/file-list.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/files', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'files', component: FileListComponent, canActivate: [authGuard] }
];

