import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddVisitorToBlacklistComponent } from './add-visitor-to-blacklist.component';

describe('AddVisitorToBlacklistComponent', () => {
  let component: AddVisitorToBlacklistComponent;
  let fixture: ComponentFixture<AddVisitorToBlacklistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddVisitorToBlacklistComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddVisitorToBlacklistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
