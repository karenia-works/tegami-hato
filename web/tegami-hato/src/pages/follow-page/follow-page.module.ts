import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FollowPageComponent } from './follow-page.component';
import { SearchbarModule } from '../../components/searchbar/searchbar.module';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [FollowPageComponent],
  imports: [
    CommonModule,
    SearchbarModule,
    RouterModule
  ],
  exports: [FollowPageComponent]
})
export class FollowPageModule {}
