import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';  
import { AppComponent } from './app/app.component';
import { provideHttpClient } from '@angular/common/http';
import { SigninComponent } from './app/signin/signin.component';
import { SignupComponent } from './app/signup/signup.component';
import { ForgotpassComponent } from './app/forgotpass/forgotpass.component';
import { DashboardComponent } from './app/dashboard/dashboard.component'; 
import { ForyouComponent } from './app/foryou/foryou.component';
import { TrendingComponent } from './app/trending/trending.component';
import { Component } from '@angular/core';
import { HomeComponent } from './app/home/home.component';
import { EditprofileComponent } from './app/editprofile/editprofile.component';
import { NewpostComponent } from './app/newpost/newpost.component';
import { SearchComponent } from './app/search/search.component';
import { FollowingComponent } from './app/following/following.component';
import { SavedComponent } from './app/saved/saved.component';
import { NotificationsComponent } from './app/notifications/notifications.component';
import { combineLatest } from 'rxjs';



const appRoutes = [
  { path: '', component: SigninComponent },
  { path: 'signup', component: SignupComponent },
  { path: 'forgotpass', component: ForgotpassComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'foryou', component: ForyouComponent},
  {path : 'trending', component: TrendingComponent},
  {path: 'home', component: HomeComponent },
   {path: 'editprofile', component: EditprofileComponent },
   {path:'newpost', component: NewpostComponent},
   {path: 'search', component: SearchComponent},
   {path: 'following', component:FollowingComponent},
   {path: 'saved', component:SavedComponent},
   {path: 'notifications', component:NotificationsComponent}

];

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(appRoutes) ,
    provideHttpClient()
  ]
}).catch((err) => console.error(err));
