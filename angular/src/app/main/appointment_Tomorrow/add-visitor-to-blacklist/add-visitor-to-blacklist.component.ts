import { AfterViewInit, Component, EventEmitter, Injector, OnInit, Output, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppointmentsServiceProxy } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-add-visitor-to-blacklist',
  templateUrl: './add-visitor-to-blacklist.component.html',
  styleUrls: ['./add-visitor-to-blacklist.component.scss']
})
export class AddVisitorToBlacklistComponent extends AppComponentBase {
  @ViewChild('addVisitorToBlacklist', { static: true }) modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
  
  active = false;
  saving = false;
  remarks : string; 
  
  appointmentId : any;
  constructor(
    injector: Injector, 
    private _appointmentsServiceProxy: AppointmentsServiceProxy,
    ){ 
      super(injector);
    }

    show(appointmentId?: any): void {
      this.appointmentId = appointmentId;
      this.active = true;
      this.modal.show();
  }
  close(): void {
      this.active = false;
      this.modal.hide();
      
  }
  AddVisitorToBlacklist(remarks : string) :void
  {
      this.saving = true;
      this._appointmentsServiceProxy
          .addVisitorToBlacklist(this.appointmentId , remarks)
          .pipe(
              finalize(() => {
                  this.saving = false;
              })
          )
          .subscribe((result) => {
              this.modalSave.emit(null);
              window.location.reload();
              this.notify.info(this.l('SuccessfullyAddVisitorToBlacklist'));
          });
      
  }   

}
