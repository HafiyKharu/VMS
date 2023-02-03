import { Component, ViewChild, Injector, Output, EventEmitter, OnInit, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';
import { LevelsServiceProxy, CreateOrEditLevelDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { DateTime } from 'luxon';

import { DateTimeService } from '@app/shared/common/timing/date-time.service';

@Component({
    selector: 'createOrEditLevelModal',
    templateUrl: './create-or-edit-level-modal.component.html',
})
export class CreateOrEditLevelModalComponent extends AppComponentBase implements OnInit {
    @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    level: CreateOrEditLevelDto = new CreateOrEditLevelDto();

    

    constructor(
        injector: Injector,
        private _levelsServiceProxy: LevelsServiceProxy,
        private _dateTimeService: DateTimeService
    ) {
        super(injector);
    }

    show(levelId?: string): void {
        if (!levelId) {
            this.level = new CreateOrEditLevelDto();
            this.level.id = levelId;

            this.active = true;
            this.modal.show();
        } else {
            this._levelsServiceProxy.getLevelForEdit(levelId).subscribe((result) => {
                this.level = result.level;

                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
        this.saving = true;

        this._levelsServiceProxy
            .createOrEdit(this.level)
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
