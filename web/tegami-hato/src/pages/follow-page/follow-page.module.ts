import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FollowPageComponent } from "./follow-page.component";
import { SearchbarModule } from '../../components/searchbar/searchbar.module';

@NgModule({
  declarations: [FollowPageComponent],
  imports: [
    CommonModule,
    SearchbarModule
  ],
  exports: [FollowPageComponent]
})
export class FollowPageModule {}
