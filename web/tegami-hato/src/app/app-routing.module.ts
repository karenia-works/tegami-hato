import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { MainPageComponent } from "src/pages/main-page/main-page.component";
import { MainPageModule } from "src/pages/main-page/main-page.module";

const routes: Routes = [
  {
    path: "",
    component: MainPageComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes), MainPageModule],
  exports: [RouterModule]
})
export class AppRoutingModule {}
