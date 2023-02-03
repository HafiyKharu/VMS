    import { AppConsts } from '@shared/AppConsts';
    import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
    import { ActivatedRoute, Router } from '@angular/router';
    import { DepartmentsServiceProxy, DepartmentDto } from '@shared/service-proxies/service-proxies';
    import { NotifyService } from 'abp-ng2-module';
    import { AppComponentBase } from '@shared/common/app-component-base';
    import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
    import { CreateOrEditDepartmentModalComponent } from './create-or-edit-department-modal.component';

    import { ViewDepartmentModalComponent } from './view-department-modal.component';
    import { appModuleAnimation } from '@shared/animations/routerTransition';
    import { Table } from 'primeng/table';
    import { Paginator } from 'primeng/paginator';
    import { LazyLoadEvent } from 'primeng/api';
    import { FileDownloadService } from '@shared/utils/file-download.service';
    import { EntityTypeHistoryModalComponent } from '@app/shared/common/entityHistory/entity-type-history-modal.component';
    import { filter as _filter } from 'lodash-es';
    import { DateTime } from 'luxon';

    import { DateTimeService } from '@app/shared/common/timing/date-time.service';

    @Component({
        templateUrl: './departments.component.html',
        encapsulation: ViewEncapsulation.None,
        animations: [appModuleAnimation()],
    })
    export class DepartmentsComponent extends AppComponentBase {
        @ViewChild('entityTypeHistoryModal', { static: true }) entityTypeHistoryModal: EntityTypeHistoryModalComponent;
        @ViewChild('createOrEditDepartmentModal', { static: true })
        createOrEditDepartmentModal: CreateOrEditDepartmentModalComponent;
        @ViewChild('viewDepartmentModalComponent', { static: true }) viewDepartmentModal: ViewDepartmentModalComponent;

        @ViewChild('dataTable', { static: true }) dataTable: Table;
        @ViewChild('paginator', { static: true }) paginator: Paginator;

        advancedFiltersAreShown = false;
        filterText = '';
        departmentNameFilter = '';

        _entityTypeFullName = 'Visitor.Department.DepartmentEnt';
        entityHistoryEnabled = false;

        constructor(
            injector: Injector,
            private _departmentsServiceProxy: DepartmentsServiceProxy,
            private _notifyService: NotifyService,
            private _tokenAuth: TokenAuthServiceProxy,
            private _activatedRoute: ActivatedRoute,
            private _fileDownloadService: FileDownloadService,
            private _dateTimeService: DateTimeService
        ) {
            super(injector);
        }

        ngOnInit(): void {
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

        getDepartments(event?: LazyLoadEvent) {
            if (this.primengTableHelper.shouldResetPaging(event)) {
                this.paginator.changePage(0);
                if (this.primengTableHelper.records && this.primengTableHelper.records.length > 0) {
                    return;
                }
            }

            this.primengTableHelper.showLoadingIndicator();

            this._departmentsServiceProxy
                .getAll(
                    this.filterText,
                    this.departmentNameFilter,
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

        createDepartment(): void {
            this.createOrEditDepartmentModal.show();
        }

        showHistory(department: DepartmentDto): void {
            this.entityTypeHistoryModal.show({
                entityId: department.id.toString(),
                entityTypeFullName: this._entityTypeFullName,
                entityTypeDescription: '',
            });
        }

        deleteDepartment(department: DepartmentDto): void {
            this.message.confirm('', this.l('AreYouSure'), (isConfirmed) => {
                if (isConfirmed) {
                    this._departmentsServiceProxy.delete(department.id).subscribe(() => {
                        this.reloadPage();
                        this.notify.success(this.l('SuccessfullyDeleted'));
                    });
                }
            });
        }

        exportToExcel(): void {
            this._departmentsServiceProxy.getAllDepartmentToExcel(this.filterText, this.departmentNameFilter).subscribe((result) => {
                this._fileDownloadService.downloadTempFile(result);
            });
        }
    }
