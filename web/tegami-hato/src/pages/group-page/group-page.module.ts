import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { GroupPageComponent } from "./group-page.component";
import { SearchbarModule } from "../../components/searchbar/searchbar.module";
import { MatIconModule } from "@angular/material/icon";

@NgModule({
  declarations: [GroupPageComponent],
  imports: [CommonModule, SearchbarModule, MatIconModule],
  exports: [GroupPageComponent]
})
export class GroupPageModule {}
