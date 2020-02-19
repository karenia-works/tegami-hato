import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ManagePageComponent } from './manage-page.component';



@NgModule({
  declarations: [ManagePageComponent],
  imports: [
    CommonModule
  ],
  exports: [ManagePageComponent]
})
export class ManagePageModule { }
