import { Routes } from '@angular/router';
import { TourListComponent } from './components/tour-list/tour-list';
import { TourLogListComponent } from './components/tour-log-list/tour-log-list';
import { StatisticsComponent } from './components/statistics/statistics';
import { ImportExportComponent } from './components/import-export/import-export';

export const routes: Routes = [
    { path: '', redirectTo: '/tours', pathMatch: 'full' },
    { path: 'tours', component: TourListComponent },
    { path: 'logs', component: TourLogListComponent },
    { path: 'statistics', component: StatisticsComponent },
    { path: 'import-export', component: ImportExportComponent }
];
