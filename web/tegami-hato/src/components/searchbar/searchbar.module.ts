import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { SearchbarComponent } from './searchbar.component';

@NgModule({
  declarations: [SearchbarComponent],
  imports: [
    CommonModule,
    MatIconModule
  ],
  exports: [SearchbarComponent]
})
export class SearchbarModule { }
