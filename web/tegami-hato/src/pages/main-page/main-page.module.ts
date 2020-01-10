import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MainPageComponent } from './main-page.component';
import { BaseComponentsModule } from 'src/components/base-components.module';

@NgModule({
  declarations: [MainPageComponent,],
  imports: [
    CommonModule,
    BaseComponentsModule,
  ],
  exports: [MainPageComponent]
})
export class MainPageModule { }
