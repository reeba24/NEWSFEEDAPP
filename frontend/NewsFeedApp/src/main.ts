import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';  
import { AppComponent } from './app/app.component';
import { provideHttpClient } from '@angular/common/http';
import { SigninComponent } from './app/signin/signin.component';
import { SignupComponent } from './app/signup/signup.component';
import { ForgotpassComponent } from './app/forgotpass/forgotpass.component';



const appRoutes = [
  { path: '', component: SigninComponent },
  { path: 'signup', component: SignupComponent },
  { path: 'forgotpass', component: ForgotpassComponent },
];

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(appRoutes) ,
    provideHttpClient()
  ]
}).catch((err) => console.error(err));
