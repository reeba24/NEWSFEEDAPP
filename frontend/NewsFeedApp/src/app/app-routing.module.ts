import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SignupComponent } from './signup/signup.component';
import { ForgotpassComponent } from './forgotpass/forgotpass.component';
import { SigninComponent } from './signin/signin.component';

export const routes: Routes = [
  { path: '', component: SigninComponent },

  { path: 'signup', component: SignupComponent },
  {path: 'forgotpass', component: ForgotpassComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }