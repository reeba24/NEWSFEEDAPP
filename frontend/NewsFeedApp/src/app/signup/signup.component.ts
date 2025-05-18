import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router'; 

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent {
  showPassword: boolean = false;
  user: any = {};
  password: string = '';
  password1: string = '';
  popupVisible: boolean = false;
  successMessage: any;

  constructor(
    private userService: UserService,
    private router: Router 
  ) {}

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  openPopup(): void {
    this.router.navigate(['/dashboard']);

    if (this.password !== this.password1) {
      this.successMessage = 'Passwords do not match.';
      this.popupVisible = true;
      return;
    }
    
    const signupData = {
      email: this.user.email,
      password: this.password,
      first_name: this.user.firstName,
      last_name: this.user.lastName,
      about: this.user.about || 'No info provided',
      created_by: 'admin'
    };
    
    this.userService.signup(signupData).subscribe({
      next: ({ message }) => {
        this.successMessage = message || 'Signup successful!';
        this.popupVisible = true;  
      },
      error: ({ error }) => {
        this.successMessage = error?.message || 'Signup failed. Please try again!';
        this.popupVisible = true; 
      }
    });
  }

  closePopup(): void {
    this.popupVisible = false;
    
  }
}
