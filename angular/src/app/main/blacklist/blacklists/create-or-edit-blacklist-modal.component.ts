﻿import { Component, ViewChild, Injector, Output, EventEmitter, OnInit, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { BlacklistsServiceProxy, CreateOrEditBlacklistDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { DateTime } from 'luxon';

import { DateTimeService } from '@app/shared/common/timing/date-time.service';

@Component({
    selector: 'createOrEditBlacklistModal',
    templateUrl: './create-or-edit-blacklist-modal.component.html',
})
export class CreateOrEditBlacklistModalComponent extends AppComponentBase implements OnInit {
    @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    blacklist: CreateOrEditBlacklistDto = new CreateOrEditBlacklistDto();

    constructor(
        injector: Injector,
        private _blacklistsServiceProxy: BlacklistsServiceProxy,
        private _dateTimeService: DateTimeService
    ) {
        super(injector);
    }

    show(blacklistId?: string): void {
        if (!blacklistId) {
            this.blacklist = new CreateOrEditBlacklistDto();
            this.blacklist.id = blacklistId;

            this.active = true;
            this.modal.show();
        } else {
            this._blacklistsServiceProxy.getblacklistForEdit(blacklistId).subscribe((result) => {
                this.blacklist = result.blacklist;

                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
        this.saving = true;

        this._blacklistsServiceProxy
            .createOrEdit(this.blacklist)
            .pipe(
                finalize(() => {
                    this.saving = false;
                })
            )
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }

    ngOnInit(): void {}
}
