import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { MainPageComponent } from "src/pages/main-page/main-page.component";
import { MainPageModule } from "src/pages/main-page/main-page.module";
import { GroupPageComponent } from "src/pages/group-page/group-page.component";
import { GroupPageModule } from "src/pages/group-page/group-page.module";
import { FollowPageComponent } from "src/pages/follow-page/follow-page.component";
import { FollowPageModule } from "src/pages/follow-page/follow-page.module";
import { ManagePageComponent } from "src/pages/manage-page/manage-page.component";
import { ManagePageModule } from "src/pages/manage-page/manage-page.module";

const routes: Routes = [
  {
    path: "",
    component: MainPageComponent
  },
  {
    path: "g",
    children: [
      {
        path: "",
        component: GroupPageComponent
      },
      {
        path: "manage",
        component: ManagePageComponent
      }
    ]
  },
  {
    path: "follow",
    component: FollowPageComponent
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes),
    MainPageModule,
    GroupPageModule,
    FollowPageModule,
    ManagePageModule
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}
