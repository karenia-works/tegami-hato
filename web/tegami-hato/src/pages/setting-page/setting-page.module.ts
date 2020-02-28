import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { SettingPageComponent } from "./setting-page.component";
import { MatIconModule } from "@angular/material/icon";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";

@NgModule({
  declarations: [SettingPageComponent],
  imports: [CommonModule, MatIconModule, FormsModule, RouterModule],
  exports: [SettingPageComponent]
})
export class SettingPageModule {}
