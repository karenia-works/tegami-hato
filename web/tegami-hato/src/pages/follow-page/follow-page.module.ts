import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FollowPageComponent } from "./follow-page.component";
import { SearchbarModule } from "../../components/searchbar/searchbar.module";
import { RouterModule } from "@angular/router";
import { MessageService } from "src/services/message/message.service";

@NgModule({
  declarations: [FollowPageComponent],
  imports: [CommonModule, SearchbarModule, RouterModule],
  providers: [MessageService],
  exports: [FollowPageComponent]
})
export class FollowPageModule {}
