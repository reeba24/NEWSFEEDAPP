import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SignupComponent } from './signup/signup.component';
import { ForgotpassComponent } from './forgotpass/forgotpass.component';
import { SigninComponent } from './signin/signin.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ForyouComponent } from './foryou/foryou.component';
import { TrendingComponent } from './trending/trending.component';

export const routes: Routes = [
  { path: '', component: SigninComponent },
  { path: 'signup', component: SignupComponent },
  {path: 'forgotpass', component: ForgotpassComponent},
  { path: 'dashboard', component: DashboardComponent },
  { path: 'foryou', component:ForyouComponent},
  { path: 'trending', component: TrendingComponent}

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }