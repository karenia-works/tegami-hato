import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GroupPageComponent } from './group-page.component';
import { SearchbarModule } from '../../components/searchbar/searchbar.module';
import { AttachmentModule } from '../../components/attachment/attachment.module';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
@NgModule({
  declarations: [GroupPageComponent],
  imports: [
    CommonModule,
    SearchbarModule,
    AttachmentModule,
    MatIconModule,
    RouterModule],
  exports: [GroupPageComponent]
})
export class GroupPageModule {}
