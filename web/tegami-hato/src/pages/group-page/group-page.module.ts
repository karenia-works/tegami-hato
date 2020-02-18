import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GroupPageComponent } from './group-page.component';
import { SearchbarModule } from '../../components/searchbar/searchbar.module';

@NgModule({
  declarations: [GroupPageComponent],
  imports: [
    CommonModule,
    SearchbarModule
  ],
  exports: [GroupPageComponent]
})
export class GroupPageModule { }
