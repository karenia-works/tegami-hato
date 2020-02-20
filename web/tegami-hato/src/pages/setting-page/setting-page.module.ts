import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { SettingPageComponent } from "./setting-page.component";
import { MatIconModule } from "@angular/material/icon";

@NgModule({
  declarations: [SettingPageComponent],
  imports: [CommonModule, MatIconModule],
  exports: [SettingPageComponent]
})
export class SettingPageModule {}
