import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EditprofileComponent } from './editprofile/editprofile.component';
import { NewpostComponent } from './newpost/newpost.component';
import { SignupComponent } from './signup/signup.component';
import { ForgotpassComponent } from './forgotpass/forgotpass.component';
import { SigninComponent } from './signin/signin.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ForyouComponent } from './foryou/foryou.component';
import { TrendingComponent } from './trending/trending.component';
import { HomeComponent } from './home/home.component';
import { SearchComponent } from './search/search.component';
import { FollowingComponent } from './following/following.component';
import { SavedComponent } from './saved/saved.component';
import { NotificationsComponent } from './notifications/notifications.component';


export const routes: Routes = [
  { path: '', component: SigninComponent },
  { path: 'signup', component: SignupComponent },
  {path: 'forgotpass', component: ForgotpassComponent},
  { path: 'dashboard', component: DashboardComponent },
  { path: 'foryou', component:ForyouComponent},
  { path: 'trending', component: TrendingComponent},
  {path: 'home', component: HomeComponent},
  {path:'editprofile', component: EditprofileComponent},
  {path:'newpost', component:NewpostComponent},
  {path:'search', component: SearchComponent},
  {path : 'following', component: FollowingComponent},
  {path: 'notifications', component: NotificationsComponent},
  {path: 'saved', component: SavedComponent}

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }