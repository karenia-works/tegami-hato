import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MainPageComponent } from './main-page.component';
import { SearchbarModule } from '../../components/searchbar/searchbar.module';

@NgModule({
  declarations: [MainPageComponent,],
  imports: [
    CommonModule,
    SearchbarModule
  ],
  exports: [MainPageComponent]
})
export class MainPageModule { }
