import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AttachmentModule } from '../../components/attachment/attachment.module';
import { MessageListComponent } from './message-list.component';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [MessageListComponent],
  imports: [
    CommonModule,
    AttachmentModule,
    RouterModule
  ],
  exports: [MessageListComponent]
})
export class MessageListModule { }
