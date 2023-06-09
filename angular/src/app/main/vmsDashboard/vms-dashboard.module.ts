import { NgModule } from '@angular/core';
import { AppSharedModule } from '@app/shared/app-shared.module';
import { AdminSharedModule } from '@app/admin/shared/admin-shared.module';
import { VMSDashboardRoutingModule } from './vms-dashboard-routing.module';
import { VMSDashboardComponent } from './vms-dashboard.component';
import { PieChartModule, AreaChartModule, LineChartModule, BarChartModule } from '@swimlane/ngx-charts';

@NgModule({
    declarations: [ VMSDashboardComponent],
    imports: [AppSharedModule,PieChartModule,AreaChartModule,LineChartModule,BarChartModule, VMSDashboardRoutingModule, AdminSharedModule],
})
export class VMSDashboardModule {}
