import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { DateTimeService } from '@app/shared/common/timing/date-time.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { VMSDashboardServiceProxy } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { filter as _filter } from "lodash-es";
import { DateTime } from "luxon";
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-vms-dashboard',
  templateUrl : './vms-dashboard.component.html',
  //styleUrls: ['./vms-dashboard.component.css'],  
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class VMSDashboardComponent extends AppComponentBase {
  @ViewChild("dataTable", { static: true }) dataTable: Table;
  @ViewChild("paginator", { static: true }) paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = "";
    fullName = "";
    maxAppointmentDateFilter: DateTime;
    minAppointmentDateFilter: DateTime;
    branchBranchNameFilter = "";
    serviceServiceNameFilter = "";
    branchIdFilter: number = 0;
    serviceIdFilter: number = 0;
    branchCodeIdFilter = "";

    appointmentDate = new Date().toLocaleDateString();
    public total = 0;
    public registered = 0;
    public checkIn = 0;
    public checkOut = 0;
    public cancel = 0;
    public miss = 0;
    completeFrac = 0;
    arriveFrac = 0;
    cancelFrac = 0;
    missFrac = 0;

  constructor(
    injector: Injector,
    private _vmsDashboardServiceProxy : VMSDashboardServiceProxy,
    private _router: Router,
    private _dateTimeService: DateTimeService,
  ) { 
    super(injector);
  }

  // ngOnInit(): void {
  // }

  getVMSDashboards(event?: LazyLoadEvent) {

    console.log(this.minAppointmentDateFilter);
    this.getAppointmentDashboard();
    if (this.primengTableHelper.shouldResetPaging(event)) {
        this.paginator.changePage(0);
        return;
    }
    //this.maxAppointmentDateFilter = this.minAppointmentDateFilter;
    this.primengTableHelper.showLoadingIndicator();
    this._vmsDashboardServiceProxy
    .getAll(
         this.filterText,
        // this.maxAppointmentDateFilter === undefined
        //     ? this.maxAppointmentDateFilter
        //     : this._dateTimeService.getEndOfDayForDate(
        //           this.maxAppointmentDateFilter
        //       ),
        // this.minAppointmentDateFilter === undefined
        //     ? this.minAppointmentDateFilter
        //     : this._dateTimeService.getStartOfDayForDate(
        //           this.minAppointmentDateFilter
        //       ),
        // this.branchBranchNameFilter,
        // this.serviceServiceNameFilter,
        this.fullName,
        this.primengTableHelper.getSorting(this.dataTable),
        this.primengTableHelper.getSkipCount(this.paginator, event),
        this.primengTableHelper.getMaxResultCount(this.paginator, event)
    )
    .subscribe((result) => {
        this.primengTableHelper.totalRecordsCount = result.totalCount;
        if (result.totalCount > 0) {
            // this.branchBranchNameFilter = result.items[0].branchBranchName;
        }
        //this.branchCodeIdFilter = "";
        this.primengTableHelper.records = result.items;
        this.primengTableHelper.hideLoadingIndicator();
    });
  }
  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
}
// createCMSDashboard(): void {
//   this._router.navigate([
//       "/app/main/appointment/cmsDashboards/createOrEdit",
//   ]);
// }
// deleteCMSDashboard(cmsDashboard: CMSDashboardDto): void {
//   this.message.confirm("", this.l("AreYouSure"), (isConfirmed) => {
//       if (isConfirmed) {
//           this._cmsDashboardsServiceProxy
//               .delete(cmsDashboard.id)
//               .subscribe(() => {
//                   this.reloadPage();
//                   this.notify.success(this.l("SuccessfullyDeleted"));
//               });
//       }
//   });
// }
getAppointmentDashboard() {
  this._vmsDashboardServiceProxy.getTotalAppointment(this.minAppointmentDateFilter)
  .subscribe(result => {
      this.total = result.totalAppointment;
      this.registered = result.registerAppointment;
      this.checkOut = result.checkOutAppointment;
      this.cancel = result.cancelAppointment;
      this.checkIn = result.checkInAppointment;
      this.miss = result.missAppointment;
      if(this.total != 0) {
          this.completeFrac =((this.checkOut)/this.total) * 100;
          this.arriveFrac = ((this.checkIn)/this.total) * 100;
          this.cancelFrac = ((this.cancel)/this.total) * 100;
          this.missFrac = ((this.miss)/this.total) * 100;
      }
  });
 
}

}
