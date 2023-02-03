import { AppConsts } from '@shared/AppConsts';
import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BlacklistsServiceProxy, BlacklistDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from 'abp-ng2-module';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditBlacklistModalComponent } from './create-or-edit-blacklist-modal.component';

import { ViewBlacklistModalComponent } from './view-blacklist-modal.component';
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
    templateUrl: './blacklists.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()],
})
export class BlacklistsComponent extends AppComponentBase {
    @ViewChild('entityTypeHistoryModal', { static: true }) entityTypeHistoryModal: EntityTypeHistoryModalComponent;
    @ViewChild('createOrEditBlacklistModal', { static: true })
    createOrEditBlacklistModal: CreateOrEditBlacklistModalComponent;
    @ViewChild('viewBlacklistModalComponent', { static: true }) viewBlacklistModal: ViewBlacklistModalComponent;

    @ViewChild('dataTable', { static: true }) dataTable: Table;
    @ViewChild('paginator', { static: true }) paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = '';
    fullNameFilter = '';

    _entityTypeFullName = 'Visitor.Blacklist.BlacklistEnt';
    entityHistoryEnabled = false;

    constructor(
        injector: Injector,
        private _blacklistsServiceProxy: BlacklistsServiceProxy,
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

    getBlacklists(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            if (this.primengTableHelper.records && this.primengTableHelper.records.length > 0) {
                return;
            }
        }

        this.primengTableHelper.showLoadingIndicator();

        this._blacklistsServiceProxy
            .getAll(
                this.filterText,
                this.fullNameFilter,
                this.primengTableHelper.getSorting(this.dataTable),
                this.primengTableHelper.getMaxResultCount(this.paginator, event),
                this.primengTableHelper.getSkipCount(this.paginator, event)
                
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

    createBlacklist(): void {
        this.createOrEditBlacklistModal.show();
    }

    showHistory(blacklist: BlacklistDto): void {
        this.entityTypeHistoryModal.show({
            entityId: blacklist.id.toString(),
            entityTypeFullName: this._entityTypeFullName,
            entityTypeDescription: '',
        });
    }

    deleteBlacklist(blacklist: BlacklistDto): void {
        this.message.confirm('', this.l('AreYouSure'), (isConfirmed) => {
            if (isConfirmed) {
                this._blacklistsServiceProxy.delete(blacklist.id).subscribe(() => {
                    this.reloadPage();
                    this.notify.success(this.l('SuccessfullyDeleted'));
                });
            }
        });
    }
    exportToExcel(): void {
        this._blacklistsServiceProxy.getAllBlacklistToExcel(this.filterText, this.fullNameFilter).subscribe((result) => {
            this._fileDownloadService.downloadTempFile(result);
        });
    }

}
