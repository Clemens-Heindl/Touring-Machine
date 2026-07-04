import { Routes } from '@angular/router';
import { TourListComponent } from './components/tour-list/tour-list';
import { StatisticsComponent } from './components/statistics/statistics';
import { ImportExportComponent } from './components/import-export/import-export';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
    { path: '', redirectTo: '/tours', pathMatch: 'full' },
    { path: 'tours', component: TourListComponent },
    { path: 'statistics', component: StatisticsComponent, canActivate: [authGuard] },
    { path: 'import-export', component: ImportExportComponent, canActivate: [authGuard] }
];
