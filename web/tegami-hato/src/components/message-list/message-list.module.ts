import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AttachmentModule } from '../../components/attachment/attachment.module';
import { MessageListComponent } from './message-list.component';

@NgModule({
  declarations: [MessageListComponent],
  imports: [
    CommonModule,
    AttachmentModule
  ],
  exports: [MessageListComponent]
})
export class MessageListModule { }
