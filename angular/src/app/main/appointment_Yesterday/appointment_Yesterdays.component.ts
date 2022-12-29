﻿import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { AppointmentsServiceProxy, AppointmentDto,CreateOrEditAppointmentDto} from '@shared/service-proxies/service-proxies';
import { TokenService } from 'abp-ng2-module';
import { AppComponentBase } from '@shared/common/app-component-base';
import { CreateOrEditAppointmentModalComponent } from './create-or-edit-appointment_Yesterday-modal.component';
import { ViewAppointmentModalComponent } from './view-appointment_Yesterday-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/table';
import { Paginator } from 'primeng/paginator';
import { LazyLoadEvent } from 'primeng/api';
import { EntityTypeHistoryModalComponent } from '@app/shared/common/entityHistory/entity-type-history-modal.component';
import { filter as _filter } from 'lodash-es';
import { DateTime } from 'luxon';
import { DateTimeService } from '@app/shared/common/timing/date-time.service';

@Component({
    templateUrl: './appointment_Yesterdays.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()],
})
export class AppointmentsComponent extends AppComponentBase {
    @ViewChild('entityTypeHistoryModal', { static: true }) entityTypeHistoryModal: EntityTypeHistoryModalComponent;
    @ViewChild('createOrEditAppointmentModal', { static: true })
    createOrEditAppointmentModal: CreateOrEditAppointmentModalComponent;
    @ViewChild('viewAppointmentModalComponent', { static: true }) viewAppointmentModal: ViewAppointmentModalComponent;

    @ViewChild('dataTable', { static: true }) dataTable: Table;
    @ViewChild('paginator', { static: true }) paginator: Paginator;

    imageUrl: any;
    imageReader = new FileReader();
    imageSrc: any;
    imageId: any;
    image: any;

    appointment: CreateOrEditAppointmentDto = new CreateOrEditAppointmentDto();
    
    advancedFiltersAreShown = false;
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
    statusFilter = -1;
    appRefNo = "";

    test:any;


    _entityTypeFullName = 'Visitor.Appointment.Appointment';
    entityHistoryEnabled = false;
    constructor(
        injector: Injector,
        private _appointmentsServiceProxy: AppointmentsServiceProxy,
        private _dateTimeService: DateTimeService,
        private _tokenService:TokenService,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.getStatus();
        this.entityHistoryEnabled = this.setIsEntityHistoryEnabled();
    }

    private setIsEntityHistoryEnabled(): boolean {
        let customSettings = (abp as any).custom;
        return (
            this.isGrantedAny('Pages.Administration.AuditLogs') &&
            customSettings.EntityHistory &&
            customSettings.EntityHistory.isEnabled &&
            _filter(
                customSettings.EntityHistory.enabledEntities,
                (entityType) => entityType === this._entityTypeFullName
            ).length === 1
        );
    }

    getAppointments(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            if (this.primengTableHelper.records && this.primengTableHelper.records.length > 0) {
                return;
            }
        }

        this.primengTableHelper.showLoadingIndicator();

        this._appointmentsServiceProxy
            .getAllYesterday(
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
                this.statusFilter,
                this.appRefNo,
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

    displayImage(appointment: AppointmentDto) {
        this._appointmentsServiceProxy.getPictureByAppointment(appointment.id).subscribe(() => {
            this.imageId = appointment.imageId;
            this._appointmentsServiceProxy.getPictureByIdOrNull(this.imageId).subscribe((result) => {
                this.imageSrc = result;
                this.image = this.imageReader.readAsDataURL(this.imageSrc)
            })
        })
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    createAppointment(): void {
        this.createOrEditAppointmentModal.show();
    }

    showHistory(appointment: AppointmentDto): void {
        this.entityTypeHistoryModal.show({
            entityId: appointment.id.toString(),
            entityTypeFullName: this._entityTypeFullName,
            entityTypeDescription: '',
        });
    }

    deleteAppointment(appointment: AppointmentDto): void {
        this.message.confirm('', this.l('AreYouSure'), (isConfirmed) => {
            if (isConfirmed) {
                this._appointmentsServiceProxy.delete(appointment.id).subscribe(() => {
                    this.reloadPage();
                    this.notify.success(this.l('SuccessfullyDeleted'));
                });
            }
        });
    }
    getStatus(appointmentId?: string):void
    {
        this._appointmentsServiceProxy.getAppointmentForEdit(appointmentId).subscribe((result) => 
            this.appointment = result.appointment);
            
        

    }
}