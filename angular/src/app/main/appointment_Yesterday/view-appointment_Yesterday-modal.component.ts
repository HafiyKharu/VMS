import { Component, ViewChild, Injector, Output, EventEmitter, Inject, LOCALE_ID } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { GetAppointmentForViewDto, AppointmentDto, AppointmentsServiceProxy, StatusType, CreateOrEditAppointmentDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { DomSanitizer } from '@angular/platform-browser';
import { finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { CheckIn } from './check-in.component';
import {  formatDate  } from '@angular/common';

@Component({
    selector: 'viewAppointmentModal',
    templateUrl: './view-appointment_Yesterday-modal.component.html',
})
export class ViewAppointmentModalComponent extends AppComponentBase {
    @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('checkIn', { static: true }) checkIn: CheckIn;
    

    currentDateTime = new Date();
    appDateTime = new Date();
    regDateTime = new Date();
    checkInDateTime = new Date();
    checkOutDateTime = new Date();
    cancelDateTime = new Date();

    parseDate(dateString: string): Date {
        return new Date(Date.parse(dateString));
    }


    
    

    active = false;
    saving = false;

    imageUrl: any;
    imageBlob: any;
    imageReader = new FileReader();
    image: any;

    item: GetAppointmentForViewDto;
    appointment: CreateOrEditAppointmentDto = new CreateOrEditAppointmentDto();
    AppointmentId: any;
    paginator: any;

    constructor(injector: Injector, private _appointmentsServiceProxy: AppointmentsServiceProxy,
        private _router: Router, private _sanitizer: DomSanitizer, @Inject(LOCALE_ID) public locale: string) {
        super(injector);
        this.item = new GetAppointmentForViewDto();
        this.item.appointment = new AppointmentDto();
    }

    displayImage(imageId: string): void {
        this._appointmentsServiceProxy.getFilePictureByIdOrNull(imageId)
            .pipe(

        )
            .subscribe((result) => {
                this.imageBlob = result;
                this.image = 'data:image/jpg;base64,' + this.imageBlob;
            });
    }

    show(item: GetAppointmentForViewDto): void {
        this.item = item;
        this._appointmentsServiceProxy.getAppointmentForEdit(item.appointment.id).subscribe((result) => {
            this.appointment = result.appointment;
            this.appointment.id = item.appointment.id;
            this.appDateTime = new Date(item.appointment.appDateTime.toString());
            this.regDateTime = new Date(item.appointment.creationTime.toString());
            this.checkInDateTime = new Date(item.appointment.checkInDateTime.toString());
            this.checkOutDateTime = new Date(item.appointment.checkOutDateTime.toString());
            this.cancelDateTime = new Date(item.appointment.cancelDateTime.toString());
        });
        this.active = true;
        this.displayImage(this.item.appointment.imageId);
        this.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }


    isStatusRegistered(): boolean {
        if (this.item.appointment.status == StatusType.Registered)
            return true;
        else
            return false;
    }
    isStatusIn(): boolean {
        if (this.item.appointment.status == StatusType.In)
            return true;
        else
            return false;
    }
    isStatusOut(): boolean {
        if (this.item.appointment.status == StatusType.Out)
            return true;
        else
            return false;
    }
    isStatusOverstayed(): boolean {
        if (this.item.appointment.status == StatusType.Overstayed)
            return true;
        else
            return false;
    }
    isStatusCancel(): boolean {
        if (this.item.appointment.status == StatusType.Cancel)
            return true;
        else
            return false;
    }

    change_Status_Register_To_Out(): void {
        this.message.confirm('', this.l('AreYouSure'), (isConfirmed) => {
            if (isConfirmed) {
                this.saving = true;
                this._appointmentsServiceProxy
                    .changeStatusToOut(this.appointment)
                    .pipe(
                        finalize(() => {
                            this.saving = false;
                        })
                    )
                    .subscribe((result) => {
                        this.modalSave.emit(null);
                        window.location.reload();
                        this.notify.info(this.l('CheckOutSuccessfully'));
                    });

            }
        });
    }
    processCancelAppointment(item: GetAppointmentForViewDto): void {
        this.item = item;
        this._appointmentsServiceProxy.getAppointmentForEdit(item.appointment.id).subscribe((result) => {
            this.appointment = result.appointment;
        });
        this.cancelAppointment();
    }
    cancelAppointment(): void {
        this.message.confirm('', this.l('AreYouSureCancelAppointment'), (isConfirmed) => {
            if (isConfirmed) {
                this.showMainSpinner();
                this.saving = true;
                this._appointmentsServiceProxy
                    .cancelAppointment(this.appointment)
                    .pipe(
                        finalize(() => {
                            this.saving = false;
                        })
                    )
                    .subscribe((result) => {
                        this.modalSave.emit(null);
                        window.location.reload();
                        this.hideMainSpinner();
                        this.notify.info(this.l('SuccessfullyCancelAppointment'));
                    });

            }
        });
    }
    save(): void {
        this.saving = true;
        this._appointmentsServiceProxy
            .createOrEdit(this.appointment)
            .pipe(
                finalize(() => {
                    this.saving = false;
                })
            )
            .subscribe((result) => {
                this.modalSave.emit(null);
            });
    }


    goToModelCheckIn(appointmentId?: string): void {

        //new this.modal1.onHide();
        this.checkIn.modal.show();
        this.checkIn.show(appointmentId);

    }
}