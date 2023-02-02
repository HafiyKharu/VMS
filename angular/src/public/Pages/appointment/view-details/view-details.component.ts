import { Injector, Output, LOCALE_ID, Inject } from '@angular/core';
import { AfterViewInit, Component, ElementRef, OnInit, ViewChild, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';
import { GetAppointmentForViewDto, AppointmentDto, PortalsServiceProxy, AppointmentsServiceProxy } from '@shared/service-proxies/service-proxies';
import { DateTime } from 'luxon';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { ReCaptchaV3Service } from 'ngx-captcha';
import { PassService } from 'public/services/pass.service';
import { combineLatest } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { FormWizardComponent } from '../form-wizard/form-wizard.component';
import { tap } from 'rxjs/operators';
import { result } from 'lodash-es';
import {  formatDate  } from '@angular/common';


@Component({
    selector: 'app-view-details',
    templateUrl: './view-details.component.html',
    styleUrls: ['./view-details.component.scss']
})
export class ViewDetailsComponent extends AppComponentBase implements OnInit, AfterViewInit {

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    uploadUrl: string;
    uploadedFiles: any[] = [];

    _formWizard: FormWizardComponent;

    appointmentId: any;
    item: GetAppointmentForViewDto;

    queryParam: any;
    reg: any;

    // item: GetAppointmentForViewDto;

    wizard: FormWizardComponent;

    constructor(injector: Injector, private route: ActivatedRoute, private _portalAppService: PortalsServiceProxy, private _appointmentAppService: AppointmentsServiceProxy, private _passService: PassService, @Inject(LOCALE_ID) public locale: string) {
        super(injector);
        this.item = new GetAppointmentForViewDto();
        this.item.appointment = new AppointmentDto();
        //this.appointmentId = this.route.snapshot.queryParams['id'];
        //this.queryParam = this.route.snapshot.queryParamMap;
        // this.appointmentId = this.queryParam.get('sendtoview') ?? undefined;
        // this.appointmentId = this.route.snapshot.data;
        //this.appointmentId = _formWizard.appId;
        // this.appointmentId = this._formWizard.appId;
    }

    show(item: GetAppointmentForViewDto): void {
        // this.item = item;
        this.active = true;
        //this.modal.show();
    }


    ngOnInit(): void {
        // this.show(this.item);
        this.showMainSpinner();
        this.appointmentId = this._passService.appointmentId;
        this.getAppointment(this.appointmentId);
        this._portalAppService.createOrEditExpiredUrl(this.appointmentId, 'Cancel').subscribe(()=> 
         this.hideMainSpinner());
    }
    ngAfterViewInit() { }


    ngOnDestroy() { }

    cancel() { }

    onBeforeSend(event): void { }

    getAppointment(appointmentId): void {
        this._appointmentAppService.getAppointmentForView(appointmentId)
            .pipe(
                tap(value => console.log(value))
            )
            .subscribe(
                (result) => {
                    result.appointment;
                    this.item.appointment = result.appointment;
                    this.item.appointment.creationTime;
                    this.reg = formatDate(this.item.appointment.creationTime.toISO(), 'dd/MM/yy, hh:mm a', this.locale,);
                    // this.item.appointment.appRefNo = result.appointment.appRefNo;
                }
            );
    }
}