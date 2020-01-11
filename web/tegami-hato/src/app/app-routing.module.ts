import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { MainPageComponent } from "src/pages/main-page/main-page.component";
import { MainPageModule } from "src/pages/main-page/main-page.module";
import { GroupPageComponent } from "src/pages/group-page/group-page.component";
import { GroupPageModule } from "src/pages/group-page/group-page.module";

const routes: Routes = [
  {
    path: "",
    component: MainPageComponent
  },
  {
    path: "g",
    component: GroupPageComponent
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes), MainPageModule,GroupPageModule],
  exports: [RouterModule]
})
export class AppRoutingModule {}
