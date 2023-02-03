import { Injector, Output } from '@angular/core';
import { AfterViewInit, Component, ElementRef, OnInit, ViewChild, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';
import { PortalsServiceProxy, CreateOrEditAppointmentDto, AppointmentsServiceProxy, StatusType, GetAppointmentForViewDto, AppointmentDto, DepartmentDto, GetDepartmentForViewDto, BlacklistsServiceProxy } from '@shared/service-proxies/service-proxies';
import { IAjaxResponse, TokenService } from 'abp-ng2-module';
import { DateTime } from 'luxon';
import { FileItem, FileUploader, FileUploaderOptions } from 'ng2-file-upload';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { ReCaptchaV3Service } from 'ngx-captcha';
import { base64ToFile, ImageCroppedEvent } from 'ngx-image-cropper';
import { PassService } from 'public/services/pass.service';
import { combineLatest } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { ViewDetailsComponent } from '../view-details/view-details.component';
import { pluck } from 'rxjs/operators';
import { ThemesLayoutBaseComponent } from '@app/shared/layout/themes/themes-layout-base.component';
import { ConvertToArrayOfStringsService } from 'public/services/convert-to-array-of-strings.service'

@Component({
    selector: 'app-form-wizard',
    templateUrl: './form-wizard.component.html',
    styleUrls: ['./form-wizard.component.scss']
})
export class FormWizardComponent extends AppComponentBase implements OnInit, AfterViewInit {

    @ViewChild('wizard', { static: true }) el: ElementRef;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('uploadPictureInputLabel') uploadPictureInputLabel: ElementRef;
    @ViewChild('viewDetailsComponent') viewDetailsComponent: ViewDetailsComponent;

    // @ViewChild('viewAppointmentModalComponent', { static: true }) viewAppointmentModal: ViewDetailsComponent;
    // @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;

    active = false;
    saving = false;
    tempGuid: any;
    appId: any;

    // appointment = { level: '' };
    // arrLevel = [    [{ level: { levelBankRakyat: 'option1' } }],
    //   [{ level: { levelBankRakyat: 'option2' } }],
    //   [{ level: { levelBankRakyat: 'option3' } }]
    // ];


    // arrayLevelString: any[];
    get sortedArrayLevelString(): any[] {
        return this.arrayLevelString.sort((a, b) => Number(a.levelBankRakyat) - Number(b.levelBankRakyat));
      }
      
      



    public uploader: FileUploader;
    public temporaryPictureUrl: string;
    public maxPictureBytesUserFriendlyValue = 5;
    imageChangedEvent: any = "";
    private _uploaderOptions: FileUploaderOptions = {};
    public uploadedFile: File;
    imageBlob: any;
    image: any;
    uploadUrl: string;

    //statusenum : Array<any> = []
    keys = Object.keys(StatusType);
    statusType: Array<string> = [];
    statusenum: typeof StatusType = StatusType;
    // statusenum = StatusType;
    appointment: CreateOrEditAppointmentDto = new CreateOrEditAppointmentDto();
    app: AppointmentDto = new AppointmentDto();
    arrPOV: Array<any> = [];
    arrTitle: Array<any> = [];
    arrTower: Array<any> = [];
    arrLevel: Array<any> = [];
    arrCompany: Array<any> = [];
    arrDepartment: Array<any> = [];

    // getdepartment: GetDepartmentForViewDto = new GetDepartmentForViewDto;
    // department: DepartmentDto = new DepartmentDto;
    arrayDepartmentString: Array<any> = [];
    arrayCompanyString: Array<any> = [];
    arrayLevelString: Array<any> = [];
    arrayTowerString: Array<any> = [];
    arrayPOVString: Array<any> = [];
    arrayTitleString: Array<any> = [];
    selectedDepartment: string;
    disabledOptionsDropdown: boolean = false;

    flatArrayDepartment: Array<string> = [];


    arrTest: Array<any> = [
        { name: 'Australia', code: 'AU' },
        { name: 'Brazil', code: 'BR' },
    ];

    fv: string = "0x0A";
    myDefaultValue: number = 1;
    sampleDateTime: DateTime;
    dateFormat = 'dd-LL-yyyy HH:mm:ss';
    r: any;
    Tower: any;
    isTower = true;
    PurposeOfVisit: any;
    isOther = true;
    detailItems: any[] = [{ title: 'user01', name: "user01" }, { title: 'user02', name: 'user02' }];
    //event for edit
    public isEditing: boolean;
    public pendingValue: string;
    public value!: string;
    public valueChangeEvents: EventEmitter<string>;
    minDate;
    maxDate;
    desc;

    //end

    constructor(
        injector: Injector,
        private _router: Router,
        private route: ActivatedRoute,
        private _portalAppService: PortalsServiceProxy,
        private _appointmentsServiceProxy: AppointmentsServiceProxy,
        private _tokenService: TokenService,
        private _reCaptchaV3Service: ReCaptchaV3Service, private router: Router,
        private _passService: PassService,
        private _arrayService: ConvertToArrayOfStringsService,
        private _blacklistAppService: BlacklistsServiceProxy) {
        super(injector);

        //event for edit
        this.isEditing = false;
        this.pendingValue = "";
        this.valueChangeEvents = new EventEmitter();
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
        this.uploadedFile = event.target.files[0];
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

                //this.appointment.imageId = resp.result.fileToken;
                //this.idPicture = resp.result.fileToken;
                //this.appointment.imageId = resp.result.fileToken;

            }
            else {
                this.message.error(resp.error.message);
            }
        };
        this.uploader.setOptions(this._uploaderOptions);
    }

    updatePicture(fileToken: string): void {
        //const input = new UpdatePictureInput();
        this.appointment.fileToken = fileToken;
        this.appointment.x = 0;
        this.appointment.y = 0;
        this.appointment.width = 0;
        this.appointment.height = 0;
        // this._appointmentsServiceProxy.updatePictureForAppointment(input)
        //     .pipe(
        //         //tap(result => this.appointment.imageId = result.toString())
        //         finalize(() => {
        //             this.saving = false;
        //         })
        //     )
        //     .subscribe((result) => {
        //         //this.active = true;
        //         this.appointment.imageId = result.toString();
        //         //abp.event.trigger('pictureChanged');
        //     })
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

    show(appointmentId?: string): void {
        if (!appointmentId) {
            this.appointment = new CreateOrEditAppointmentDto();
            this.appointment.id = appointmentId;
        }
    }

    upload(): void {
        this.uploader.uploadAll();
        this.notify.info(this.l('UploadSuccessfully'));
    }

    reset(form: NgForm): void {
        this.message.confirm(this.l('FormResetWarningMessage'), this.l('AreYouSure'), (isConfirmed) => {
            if (isConfirmed) {
                this.uploadPictureInputLabel.nativeElement.innerText = "";
                this.uploadPictureInputLabel.nativeElement.value = "";
                this.imageChangedEvent = null;
                form.reset();
            }
        });
    }

    save(icOrPassportNumber : string, form: NgForm): void {
        this._blacklistAppService.isExisted(icOrPassportNumber).subscribe(x => {
            if (x) 
            {
                this.message.error( this.l('BlacklistInfo2'));
                this.saving = false;
                this.uploadPictureInputLabel.nativeElement.innerText = "";
                this.uploadPictureInputLabel.nativeElement.value = "";
                this.imageChangedEvent = null;
                form.reset();
            }
            else
            {
                this.saving = true;
                this._portalAppService
                    .createOrEdit(this.appointment)
                    .pipe(
                        finalize(() => {
                            this.saving = false;
                        }),
                        pluck('result')
                    )
                    .subscribe((result) => {
                        this.notify.info(this.l('AppointmentCreatedSuccessfully'));
                        this.modalSave.emit(null);
                        this._passService.appointmentId = result;
                        this.viewDetails();
                        this.hideMainSpinner();
                    });
            }
        })
    }

    getArray(): void {

        combineLatest
            (
                this._appointmentsServiceProxy.getTitle(), this._appointmentsServiceProxy.getTower(),
                this._appointmentsServiceProxy.getPurposeOfVisit(), this._appointmentsServiceProxy.getLevel(),
                this._appointmentsServiceProxy.getCompanyName(), this._appointmentsServiceProxy.getDepartmentName()
            )
            .subscribe(([valueTitle, valueTower, valuePOV, valueLevel, valueCompany, valueDepartment]) => {
                this.arrTitle.push(valueTitle),
                    this.arrTower.push(valueTower),
                    this.arrPOV.push(valuePOV),
                    this.arrLevel.push(valueLevel),
                    this.arrCompany.push(valueCompany),
                    this.arrDepartment.push(valueDepartment);
                this.convertToArrayString();
            })
    }

    convertToArrayString(): void {
        // this.arrayDepartmentString = this._arrayService.getArrayString(this.arrDepartment);
        // for (var i = 0; i < this.arrDepartment.length; i++) {
        //     this.arrDepartment.map((res, i) => this.arrayDepartmentString.push(res[i].department.departmentName));
        // }

        this.arrDepartment.map((res) => {
            res.map((result) => {
                this.arrayDepartmentString.push(result.department);
            })
        });
        this.arrayDepartmentString.sort((a, b) => a.departmentName.localeCompare(b.departmentName));

        this.arrCompany.map((res) => {
            res.map((result) => {
                this.arrayCompanyString.push(result.company);
            })
        });
        this.arrayCompanyString.sort((a, b) => a.companyName.localeCompare(b.companyName));

        this.arrLevel.map((res) => {
            res.map((result) => {
                this.arrayLevelString.push(result.level);
            })
        });

        this.arrayLevelString.sort((a, b) => a.levelBankRakyat.localeCompare(b.levelBankRakyat));

        this.arrTower.map((res) => {
            res.map((result) => {
                this.arrayTowerString.push(result.tower);
            })
        });
        this.arrayTowerString.sort((a, b) => a.towerBankRakyat.localeCompare(b.towerBankRakyat));

        this.arrPOV.map((res) => {
            res.map((result) => {
                this.arrayPOVString.push(result.purposeOfVisit);
            })
        });
        this.arrayPOVString.sort((a, b) => a.purposeOfVisitApp.localeCompare(b.purposeOfVisitApp));

        this.arrTitle.map((res) => {
            res.map((result) => {
                this.arrayTitleString.push(result.title);
            })
        });
        this.arrayTitleString.sort((a, b) => a.visitorTitle.localeCompare(b.visitorTitle));

    }

    ngOnInit(appointmentId?: string): void {
        this.getArray();
        this.initializeModal();
        this.minDate = new Date();
        this.maxDate = new Date();
        this.maxDate.setMonth(this.maxDate.getMonth() + 3);
        
        //window.location.
    }
    ngAfterViewInit() {
    }


    ngOnDestroy() {
    }

    cancel() { }

    viewDetails() {
        this._router.navigateByUrl('/appointment-details');
    }
    onChange(getValueTower) {
        this.Tower = getValueTower;
        if (this.Tower == "Tower 1") {
            this.appointment.companyName = "Bank Rakyat";
            this.arrayCompanyString.forEach((res) => {
                if (res.companyName == "Bank Rakyat") {
                    this.disabledOptionsDropdown = true;
                }
            })
        }
        else if (this.Tower == "Tower 2") {
            this.disabledOptionsDropdown = false;
        }
    }

}