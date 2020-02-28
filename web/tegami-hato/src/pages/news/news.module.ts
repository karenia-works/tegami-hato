import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NewsComponent } from './news.component';
import { MessageListModule } from '../../components/message-list/message-list.module';
import { SearchbarModule } from '../../components/searchbar/searchbar.module';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [NewsComponent],
  imports: [
    CommonModule,
    MessageListModule,
    RouterModule,
    SearchbarModule
  ],
  exports: [NewsComponent]
})
export class NewsModule {}
