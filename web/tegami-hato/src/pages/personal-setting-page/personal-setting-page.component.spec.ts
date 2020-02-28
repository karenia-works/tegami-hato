import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PersonalSettingPageComponent } from './personal-setting-page.component';

describe('PersonalSettingPageComponent', () => {
  let component: PersonalSettingPageComponent;
  let fixture: ComponentFixture<PersonalSettingPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PersonalSettingPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PersonalSettingPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
