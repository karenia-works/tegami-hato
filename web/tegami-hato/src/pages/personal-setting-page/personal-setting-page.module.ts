import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PersonalSettingPageComponent } from './personal-setting-page.component';



@NgModule({
  declarations: [PersonalSettingPageComponent],
  imports: [
    CommonModule
  ],
  exports: [PersonalSettingPageComponent]
})
export class PersonalSettingPageModule { }
