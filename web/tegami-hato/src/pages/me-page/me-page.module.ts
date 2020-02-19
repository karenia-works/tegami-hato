import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MePageComponent } from './me-page.component';

@NgModule({
  declarations: [MePageComponent],
  imports: [
    CommonModule
  ],
  exports: [MePageComponent]
})
export class MePageModule { }
