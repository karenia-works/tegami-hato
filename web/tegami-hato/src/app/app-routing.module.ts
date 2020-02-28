import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MainPageModule } from 'src/pages/main-page/main-page.module';
import { MainPageComponent } from 'src/pages/main-page/main-page.component';
import { GroupPageModule } from 'src/pages/group-page/group-page.module';
import { GroupPageComponent } from 'src/pages/group-page/group-page.component';
import { FollowPageModule } from 'src/pages/follow-page/follow-page.module';
import { FollowPageComponent } from 'src/pages/follow-page/follow-page.component';
import { LoginPageModule } from 'src/pages/login-page/login-page.module';
import { LoginPageComponent } from 'src/pages/login-page/login-page.component';
import { SettingPageModule } from 'src/pages/setting-page/setting-page.module';
import { SettingPageComponent } from 'src/pages/setting-page/setting-page.component';
import { MePageModule } from 'src/pages/me-page/me-page.module';
import { MePageComponent } from 'src/pages/me-page/me-page.component';
import { NewsModule } from 'src/pages/news/news.module';
import { NewsComponent } from 'src/pages/news/news.component';


const routes: Routes = [
  {
    path: '',
    component: MainPageComponent
  },
  {
    path: 'follow',
    component: FollowPageComponent
  },
  {
    path: 'channel/:channelId',
    component: GroupPageComponent
  },
  {
    path: 'channel/:channelId/setting',
    component: SettingPageComponent
  },
  {
    path: 'login',
    component: LoginPageComponent
  },
  {
    path: 'me',
    component: MePageComponent
  },
  {
    path: 'news',
    component: NewsComponent
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes),
    MainPageModule,
    GroupPageModule,
    FollowPageModule,
    LoginPageModule,
    SettingPageModule,
    NewsModule,
    MePageModule
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}
