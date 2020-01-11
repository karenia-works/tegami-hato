import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GroupPageComponent } from './group-page.component';
import { BaseComponentsModule } from 'src/components/base-components.module';




@NgModule({
  declarations: [GroupPageComponent],
  imports: [
    CommonModule,
    BaseComponentsModule
  ],
  exports: [GroupPageComponent]
})
export class GroupPageModule { }
