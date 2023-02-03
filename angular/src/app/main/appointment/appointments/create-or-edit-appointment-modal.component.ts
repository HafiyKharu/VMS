import { Component, ViewChild, Injector, Output, EventEmitter, OnInit, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize, map, min, switchMap, tap } from 'rxjs/operators';
import { AppointmentsServiceProxy, BlacklistsServiceProxy, CreateOrEditAppointmentDto, DepartmentDto, GetDepartmentForViewDto, StatusType, } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { DateTime } from 'luxon';
import { DatePipe } from '@angular/common'

import { DateTimeService } from '@app/shared/common/timing/date-time.service';
import { result } from 'lodash-es';
import { key } from 'localforage';
import { AppConsts } from '@shared/AppConsts';
import { FileItem, FileUploader, FileUploaderOptions } from 'ng2-file-upload';
import { base64ToFile, ImageCroppedEvent } from 'ngx-image-cropper';
import { IAjaxResponse, TokenService } from 'abp-ng2-module';
import { Observable } from 'rxjs';
import { resolve } from 'path';


@Component({
    selector: 'createOrEditAppointmentModal',
    templateUrl: './create-or-edit-appointment-modal.component.html',
})
export class CreateOrEditAppointmentModalComponent extends AppComponentBase implements OnInit {

    @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;
    @ViewChild('uploadPictureInputLabel') uploadPictureInputLabel: ElementRef;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    tempGuid: any;
    public uploader: FileUploader;
    public temporaryPictureUrl: string;
    public maxPictureBytesUserFriendlyValue = 5;
    imageChangedEvent: any = "";
    private _uploaderOptions: FileUploaderOptions = {};
    public uploadedFile: File;
    imageBlob: any;
    image: any;
    uploadUrl: string;
    uploadedFiles: any[] = [];
    appId: any;
    keys = Object.keys(StatusType);
    statusType: Array<string> = [];
    statusenum: typeof StatusType = StatusType;
    appointment: CreateOrEditAppointmentDto = new CreateOrEditAppointmentDto();
    arrPOV: Array<any> = [];
    arrTitle: Array<any> = [];
    arrTower: Array<any> = [];
    arrLevel: Array<any> = [];
    arrCompany: Array<any> = [];
    arrDepartment: Array<any> = [];
    fv: string = "0x0A";
    myDefaultValue: number = 1;
    sampleDateTime: DateTimeService;
    dateFormat = 'dd-LL-yyyy HH:mm:ss';
    r: any;
    detailItems: any[] = [{ title: 'user01', name: "user01" }, { title: 'user02', name: 'user02' }];
    //event for edit
    public isEditing: boolean;
    public pendingValue: string;
    public value!: string;
    public valueChangeEvents: EventEmitter<string>;
    Tower: any;
    isTower = true;
    minDate;
    maxDate;

    constructor(
        injector: Injector,
        private _appointmentsServiceProxy: AppointmentsServiceProxy,
        private _dateTimeService: DateTimeService,
        private _tokenService: TokenService,
        private _blacklistAppService: BlacklistsServiceProxy,
        //public datepipe: DatePipe
    ) {
        super(injector);
        //this.uploadUrl = AppConsts.remoteServiceBaseUrl + '/Appointment/UploadAppointmentPicture';
    }

    initializeModal(): void {
        this.active = true;
        this.temporaryPictureUrl = '';
        this.initFileUploader();
    }

    fileChangeEvent(event: any): void {
        if (event.target.files[0].size > 5242880) {
            //5MB
            this.message.warn(this.l('ProfilePicture_Warn_SizeLimit', this.maxPictureBytesUserFriendlyValue));
            return;
        }

        this.uploadPictureInputLabel.nativeElement.innerText = event.target.files[0].name;

        this.imageChangedEvent = event;
    }

    imageCroppedFile(event: ImageCroppedEvent) {
        this.uploader.clearQueue();
        this.uploader.addToQueue([<File>base64ToFile(event.base64)]);
        this.uploadUrl = AppConsts.remoteServiceBaseUrl + '/Appointment/UploadAppointmentPicture';

        //event for edit
        this.isEditing = false;
        this.pendingValue = "";
        this.valueChangeEvents = new EventEmitter();
    }

    initFileUploader(): void {
        this.uploader = new FileUploader({ url: AppConsts.remoteServiceBaseUrl + '/Appointment/UploadAppointmentPicture' });
        this._uploaderOptions.autoUpload = true;
        this._uploaderOptions.authToken = 'Bearer ' + this._tokenService.getToken();
        this._uploaderOptions.removeAfterUpload = true;
        this.uploader.onAfterAddingFile = (file) => {
            file.withCredentials = false;
        };

        this.uploader.onBuildItemForm = (fileItem: FileItem, form: any) => {
            this.guid();
            form.append('FileType', fileItem.file.type);
            form.append('FileName', 'AppointmentPicture');
            form.append('FileToken', this.tempGuid);
            //form.append('uploadFile', this.uploadedFile);
            //this.appointment.imageId = this.tempGuid;
        };

        // onSuccessItem run after item is success eg: after fx this.uploader.uploadAll()

        // this.uploader.onBeforeUploadItem = (fileItem: FileItem) => {
        //     fileItem._onSuccess = (response, status) => {
        //         const resp = <IAjaxResponse>JSON.parse(response);
        //         //this.appointment.imageId = resp.result.fileToken;
        //         this.updatePicture(resp.result.fileToken);
        //     }
        // }

        // this.uploader.uploadItem = (fileItem: FileItem) => {
        //     this.guid();
        //     this.updatePicture(this.tempGuid);
        // }

        // this.uploader.onCompleteItem = (item, response, status) => {
        //     const resp = <IAjaxResponse>JSON.parse(response);
        //     if (resp.success) {
        //         //this.appointment.imageId = resp.result.id;
        //     }
        // }

        this.uploader.onSuccessItem = (item, response, status) => {
            const resp = <IAjaxResponse>JSON.parse(response);
            if (resp.success) {
                this.updatePicture(resp.result.fileToken);
            }
            else {
                this.message.error(resp.error.message);
            }
        };
        this.uploader.setOptions(this._uploaderOptions);
    }

    updatePicture(fileToken: string): void {
        // const input = new UpdatePictureInput();
        this.appointment.fileToken = fileToken;
        this.appointment.x = 0;
        this.appointment.y = 0;
        this.appointment.width = 0;
        this.appointment.height = 0;
        // input.fileToken = fileToken;
        // input.x = 0;
        // input.y = 0;
        // input.width = 0;
        // input.height = 0;

        //this.saving = true;
        // this._appointmentsServiceProxy.updatePictureForAppointment(input)
        //     .pipe(
        //         //tap(result => this.appointment.imageId = result.toString())
        //         // finalize(() => {
        //         //     this.saving = false;
        //         // })
        //         map((data) => data.toString())
        //     )
        //     .subscribe((result) => {
        //         //this.active = true;
        //         this.appointment.imageId = result.toString();
        //         //abp.event.trigger('pictureChanged');
        //     })
        // return this.appointment;
    }

    guid(): string {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }

        this.tempGuid = s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
        return this.tempGuid;
    }

    displayImage(imageId: string): void {
        this._appointmentsServiceProxy.getFilePictureByIdOrNull(imageId)
            .subscribe((result) => {
                this.imageBlob = result;
                //this.image = this.imageReader.readAsDataURL(this.imageBlob);
                this.image = 'data:image/jpg;base64,' + this.imageBlob;
            });
    }

    // checkPictureExistOrNot(imageId: string) {
    //     if(!imageId) {
    //         return "You dont have any image";
    //     }
    //     else {
    //         this._appointmentsServiceProxy.getFilePictureByIdOrNull(imageId)
    //         .subscribe((result) => {
    //             this.convertFromBase64ToBlob(result);
    //         })
    //         this.uploadedFile = new File([this.imageBlob], "Appointment.png",{type: "png/jpeg"});
    //         return this.uploadedFile;
    //     }
    // }

    // convertFromBase64ToBlob(base64: string) {
    //     var contentType = contentType || '';
    //     const sliceSize = 512;
    //     const byteCharacters = window.atob(base64.replace(/^data:image\/(png|jpeg|jpg);base64,/, ''));
    //     const byteArrays = [];

    //     for (let offset = 0; offset < byteCharacters.length; offset += sliceSize){
    //         const slice = byteCharacters.slice(offset, offset + sliceSize);

    //         const byteNumbers = new Array(slice.length);
    //         for (let i = 0; i < slice.length; i++){
    //             byteNumbers[i] = slice.charCodeAt(i);
    //         }

    //         const byteArray = new Uint8Array(byteNumbers);
    //         byteArrays.push(byteArray);
    //     }
    //     this.imageBlob = new Blob(byteArrays, {type: contentType});
    //     return this.imageBlob;
    // }

    show(appointmentId?: string): void {
        this.minDate = new Date();
        this.maxDate = new Date();
        this.maxDate.setMonth(this.maxDate.getMonth() + 3);
        this.initializeModal();
        this.modal.show();
        if (!appointmentId) {
            this.GetEmptyArray();
            this.getPOVArray();
            this.getTitleArray();
            this.getTowerArray();
            this.getCompanyArray();
            this.getDepartmentArray();
            this.getLevelArray();
            this.getStatusEnum();
            this.appointment = new CreateOrEditAppointmentDto();
            this.appointment.id = appointmentId;
            this.active = true;
            this.modal.show();
        } else {
            this._appointmentsServiceProxy.getAppointmentForEdit(appointmentId).subscribe((result) => {
                this.appointment = result.appointment;
                this.getPOVArray();
                this.getLevelArray();
                this.getTitleArray();
                this.getTowerArray();
                this.getCompanyArray();
                this.getDepartmentArray();
                this.getStatusEnum();

                this.active = true;
                this.modal.show();
                this.displayImage(this.appointment.imageId);
                // this.checkPictureExistOrNot(this.appointment.imageId);
                // this.uploadedFile;
            });
        }
    }

    // upload(): void {
    //     this.uploader.uploadAll();
    //     this.notify.info(this.l('UploadSuccessfully'));
    // }

    save(icOrPassportNumber : string): void {
        this._blacklistAppService.isExisted(icOrPassportNumber).subscribe(x => {
            if (x) 
            {
                this.message.error( this.l('BlacklistInfo1'));
                this.saving = false;
                this.close();
            }
            else
            {
                this.saving = true;
                this.uploader.uploadAll();
                this._appointmentsServiceProxy
                    .createOrEdit(this.appointment)
                    .pipe(
                        finalize(() => {
                            this.saving = false;
                        })
                    )
                    .subscribe(() => {
                        // var pic = this._appointmentsServiceProxy.updatePictureForAppointment(this.resInput)
                        // .subscribe((result) => {
                        //     this.appointment.imageId = result.toString();
                        // })
                        this.notify.info(this.l('SavedSuccessfully'));
                        this.close();
                        this.modalSave.emit(null);
                    });
        
                //another approach for image upload
                // this._appointmentsServiceProxy
                // .updatePictureForAppointment(this.resInput)
                // .subscribe((result) => {
                //     this.appointment.imageId = result.toString();
                //     this._appointmentsServiceProxy
                //     .createOrEdit(this.appointment)
                //     .pipe(
                //         finalize(() => {
                //             this.saving = false;
                //         })
                //     )
                //     .subscribe(() => {
                //         this.notify.info(this.l('SavedSuccessfully'));
                //         this.close();
                //         this.modalSave.emit(null);
                //     })
                // })
            }
        
        })

    }

    close(): void {
        this.uploadedFiles = [];
        this.active = false;
        this.imageChangedEvent = '';
        this.modal.hide();
    }

    ngOnInit(): void {
    }

    //ListPurposeOfVisit
    getPOVArray(): void {
        this._appointmentsServiceProxy.getPurposeOfVisit().subscribe((result) => {
            this.arrPOV = [];
            this.arrPOV.push(result);
        })
    }
    //List title
    getTitleArray(): void {
        this._appointmentsServiceProxy.getTitle().subscribe((result) => {
            this.arrTitle = [];
            this.arrTitle.push(result);
        })
    }
    //List tower
    getTowerArray(): void {
        this._appointmentsServiceProxy.getTower().subscribe((result) => {
            this.arrTower = [];
            this.arrTower.push(result);
        })
    }
    //List level
    getLevelArray(): void {
        this._appointmentsServiceProxy.getLevel().subscribe((result) => {
            this.arrLevel = [];
            this.arrLevel.push(result);
        })
    }
    //List company name
    getCompanyArray(): void {
        this._appointmentsServiceProxy.getCompanyName().subscribe((result) => {
            this.arrCompany = [];
            this.arrCompany.push(result);
        })
    }
    //List Department Name
    getDepartmentArray(): void {
        this._appointmentsServiceProxy.getDepartmentName().subscribe((result) => {
            this.arrDepartment = [];
            this.arrDepartment.push(result)
        })
    }
    GetEmptyArray(): void {
        this.arrDepartment = [];
        this.arrCompany = [];
        this.arrLevel = [];
        this.arrTower = [];
        this.arrTitle = [];
        this.arrPOV = [];
    }
    getStatusEnum(): void {
        this.statusType = [];
        for (let s in StatusType) {
            if (isNaN(Number(s))) {
                this.statusType.push(s);
            }
        };
    }

    // onUpload(event): void {
    //     // for (const file of event.files) {
    //     //     this.uploadedFiles.push(file);
    //     // }
    //     this.uploadedFiles = event.files;
    //     for (const files of event.originalEvent.body.result) {
    //         this.appointment.imageId = files.id;
    //     }

    //     //event.originalEvent.body.result[0].id (expression)
    // }

    onBeforeSend(event): void {
        event.xhr.setRequestHeader('Authorization', 'Bearer' + abp.auth.getToken());
    }

    public cancel(): void {

        this.isEditing = false;

    }


    // I enable the editing of the value.
    public edit(): void {

        this.pendingValue = this.value;
        this.isEditing = true;

    }


    // I process changes to the pending value.
    public processChanges(): void {

        // If the value actually changed, emit the change but don't change the local
        // value - we don't want to break unidirectional data-flow.
        if (this.pendingValue !== this.value) {

            this.valueChangeEvents.emit(this.pendingValue);

        }

        this.isEditing = false;

    }

    editDateTime(Appointment_AppDateTime) {
        Appointment_AppDateTime.tekan = true
    }
    setTitleEdit() {
        this.isEditing = true;
    }
    public open(event: any, item: string) {
        alert('Open ' + item);
    }
    onChange(getValueTower) 
    {
        this.Tower = getValueTower;
        if (this.Tower == "Tower 1")
            this.isTower = false;
        else
        this.isTower = true;
    }
}