import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidateIccStartComponent } from './validate-icc-start.component';

describe('ValidateIccStartComponent', () => {
  let component: ValidateIccStartComponent;
  let fixture: ComponentFixture<ValidateIccStartComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ValidateIccStartComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ValidateIccStartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
