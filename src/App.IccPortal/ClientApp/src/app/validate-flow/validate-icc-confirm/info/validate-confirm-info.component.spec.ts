import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidateConfirmInfoComponent } from './validate-confirm-info.component';

describe('ValidateStep4Component', () => {
  let component: ValidateConfirmInfoComponent;
  let fixture: ComponentFixture<ValidateConfirmInfoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ValidateConfirmInfoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ValidateConfirmInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
