import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { SharePageComponent } from "./share-page.component";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

@NgModule({
  declarations: [SharePageComponent],
  imports: [CommonModule, FormsModule],
  exports: [SharePageComponent]
})
export class SharePageModule {}
