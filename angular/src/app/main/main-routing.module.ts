import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                children: [

                    {
                        path: 'appointment/appointments',
                        loadChildren: () => import('./appointment/appointments/appointment.module').then((m) => m.AppointmentModule),
                        data: { permission: 'Pages.Appointments' },
                    },
                    {
                        path: 'appointment_Today',
                        loadChildren: () => import('./appointment_Today/appointment_Today.module').then((m) => m.AppointmentTodayModule),
                        data: { permission: 'Pages.Appointments' },
                    },
                    {
                        path: 'appointment_Tomorrow',
                        loadChildren: () => import('./appointment_Tomorrow/appointment_Tomorrow.module').then((m) => m.AppointmentTomorrowModule),
                        data: { permission: 'Pages.Appointments' },
                    },
                    {
                        path: 'appointment_Yesterday',
                        loadChildren: () => import('./appointment_Yesterday/appointment_Yesterday.module').then((m) => m.AppointmentYesterdayModule),
                        data: { permission: 'Pages.Appointments' },
                    },
                    {
                        path: 'department/departments',
                        loadChildren: () => import('./department/departments/department.module').then((m) => m.DepartmentModule),
                        data: { permission: 'Pages.Departments' },
                    },

                    
                    {
                        path: 'company/companies',
                        loadChildren: () => import('./company/companies/company.module').then(m => m.CompanyModule),
                        data: { permission: 'Pages.Companies' }
                    },
                
                    {
                        path: 'blacklist/blacklists',
                        loadChildren: () => import('./blacklist/blacklists/blacklist.module').then((m) => m.BlacklistModule),
                        data: { permission: 'Pages.Blacklists' },
                    },
                    {
                        path: 'title/titles',
                        loadChildren: () => import('./title/titles/title.module').then((m) => m.TitleModule),
                        data: { permission: 'Pages.Titles' },
                    },
                    {
                        path: 'purposeOfVisit/purposeOfVisits',
                        loadChildren: () => import('./purposeOfVisit/purposeOfVisits/purposeOfVisit.module').then((m) => m.PurposeOfVisitModule),
                        data: { permission: 'Pages.PurposeOfVisits' },
                    },
                    {
                        path: 'tower/towers',
                        loadChildren: () => import('./tower/towers/tower.module').then((m) => m.TowerModule),
                        data: { permission: 'Pages.Towers' },
                    },

                    {
                        path: 'level/levels',
                        loadChildren: () => import('./level/levels/level.module').then((m) => m.LevelModule),
                        data: { permission: 'Pages.Levels' },
                    },
                    {
                        path: 'vmsDashboard',
                        loadChildren: () => import('./vmsDashboard/vms-dashboard.module').then((m) => m.VMSDashboardModule),
                        data: { permission: 'Pages.VMSDashboards' },
                    },
                    {
                        path: 'dashboard',
                        loadChildren: () => import('./dashboard/dashboard.module').then((m) => m.DashboardModule),
                        data: { permission: 'Pages.Tenant.Dashboard' },
                    },
                    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
                    { path: '**', redirectTo: 'dashboard' },
                ],
            },
        ]),
    ],
    exports: [RouterModule],
})
export class MainRoutingModule { }
