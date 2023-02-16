import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { DateTimeService } from '@app/shared/common/timing/date-time.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { StatusType, VMSDashboardServiceProxy } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { filter as _filter } from "lodash-es";
import { DateTime } from "luxon";
import { appModuleAnimation } from '@shared/animations/routerTransition';

@Component({
  selector: 'app-vms-dashboard',
  templateUrl : './vms-dashboard.component.html',
  styleUrls: ['./vms-dashboard.component.css'],  
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class VMSDashboardComponent extends AppComponentBase {
  @ViewChild("dataTable", { static: true }) dataTable: Table;
  @ViewChild("paginator", { static: true }) paginator: Paginator;

    maxAppointmentDateFilter: DateTime;
    minAppointmentDateFilter: DateTime;
    branchBranchNameFilter = "";
    serviceServiceNameFilter = "";
    branchIdFilter: number = 0;
    serviceIdFilter: number = 0;
    branchCodeIdFilter = "";

    advancedFiltersAreShown = false;
    appointmentDate = new Date().toLocaleDateString();
    public total = 0;
    public registered = 0;
    public checkIn = 0;
    public checkOut = 0;
    public cancel = 0;
    checkOutFrac = 0;
    checkInFrac = 0;
    cancelFrac = 0;
    registeredFrac = 0;

    filterText = '';
    fullNameFilter = '';
    identityCardFilter = '';
    phoneNoFilter = '';
    emailFilter = '';
    titleFilter = '';
    companyNameFilter = '';
    officerToMeetFilter = '';
    purposeOfVisitFilter = '';
    departmentFilter = '';
    towerFilter = '';
    levelFilter = '';
    minAppDateTimeFilter: DateTime;
    maxAppDateTimeFilter: DateTime;
    minRegDateTimeFilter: DateTime;
    maxRegDateTimeFilter: DateTime;
    statusFilter: any;
    passNumberFilter = '';
    appRefNoFilter = "";
    emailOfficerToMeetFilter = "";
    phoneNoOfficerToMeetFilter = "";

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
    .getAllTodayVMSDashboard(
      this.filterText,
      this.fullNameFilter,
      this.identityCardFilter,
      this.phoneNoFilter,
      this.emailFilter,
      this.titleFilter,
      this.companyNameFilter,
      this.officerToMeetFilter,
      this.purposeOfVisitFilter,
      this.departmentFilter,
      this.towerFilter,
      this.levelFilter,
      this.minAppDateTimeFilter,
      this.maxAppDateTimeFilter,
      this.minRegDateTimeFilter,
      this.maxRegDateTimeFilter,
      this.emailOfficerToMeetFilter,
      this.phoneNoOfficerToMeetFilter,
      this.statusFilter,
      this.passNumberFilter,
      this.appRefNoFilter,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
    )
    .subscribe((result) => {
        this.primengTableHelper.totalRecordsCount = result.totalCount;
        this.primengTableHelper.records = result.items;
        this.primengTableHelper.hideLoadingIndicator();
    });
  }
  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
}
getAppointmentDashboard() {
  this._vmsDashboardServiceProxy.getTotalAppointment(this.minAppointmentDateFilter)
  .subscribe(result => {

      this.total = result.totalToday;
      this.registered = result.register;
      this.checkOut = result.checkOut;
      this.cancel = result.cancel;
      this.checkIn = result.checkIn;

      if(this.total != 0) {
        this.registeredFrac = ((this.registered)/this.total) * 100;
          this.checkOutFrac =((this.checkOut)/this.total) * 100;
          this.checkInFrac = ((this.checkIn)/this.total) * 100;
          this.cancelFrac = ((this.cancel)/this.total) * 100;
      }
  });
}
isStatusRegistered(status: any): boolean {
  if (status == StatusType.Registered) {
      return true;
  }
  else
      return false;
}
isStatusIn(status: any): boolean {
  if (status == StatusType.In)
      return true;
  else
      return false;
}
isStatusOut(status: any): boolean {
  if (status == StatusType.Out)
      return true;
  else
      return false;
}
isStatusCancel(status: any): boolean {
  if (status == StatusType.Cancel)
      return true;
  else
      return false;
}
checkStatusOverstayed(CheckInDateTime: any, status: any): boolean {
  if (status == StatusType.In) {
      let dateOnlyString = new Date(CheckInDateTime);
      let dateOnly = new Date(dateOnlyString);
      let time = new Date();
      let dateNow = new Date();
      time.setHours(19);
      time.setMinutes(0);
      time.setSeconds(0);

      let combinedDate = new Date(dateOnly.getFullYear(), dateOnly.getMonth(), dateOnly.getDate(), time.getHours(), time.getMinutes(), time.getSeconds());

      if (dateNow >= combinedDate)
          return true;
      
  }
}
isStatusExpired(AppDateTime: any, status: any): boolean {
  if (status == StatusType.Registered) {
      let appDateTime = new Date(AppDateTime);
      let dateNow = new Date();

      if (dateNow.getDate > appDateTime.getDate)
          return true;
      
  }
}

}
