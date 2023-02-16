import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { VMSDashboardComponent } from './vms-dashboard.component';
const routes: Routes = [
    {
        path: '',
        component: VMSDashboardComponent,
        pathMatch: 'full',
    },
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class VMSDashboardRoutingModule {}
