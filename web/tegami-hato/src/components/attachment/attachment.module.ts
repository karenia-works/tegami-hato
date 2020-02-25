import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AttachmentComponent } from './attachment.component';

@NgModule({
  declarations: [AttachmentComponent],
  imports: [
    CommonModule,
    MatIconModule
  ],
  exports: [AttachmentComponent]
})
export class AttachmentModule { }
