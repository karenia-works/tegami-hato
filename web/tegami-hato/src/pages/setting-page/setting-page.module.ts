import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SettingPageComponent } from './setting-page.component';

@NgModule({
  declarations: [SettingPageComponent],
  imports: [
    CommonModule
  ],
  exports: [SettingPageComponent]
})
export class SettingPageModule { }
