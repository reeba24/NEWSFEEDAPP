import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';

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
  constructor(private userService: UserService) {}
  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  openPopup(): void {
    if (this.password !== this.password1) {
      alert('Passwords do not match.');
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
      next: (response) => {
        console.log(response);
        this.successMessage = response.message || 'Signup successful!';
        this.popupVisible = true;
      },
      error: (error) => {
        console.error('Signup failed:', error);
        if (error.status === 400 && error.error?.errors) {
          const backendErrors = error.error.errors;
          for (const field in backendErrors) {
            if (backendErrors.hasOwnProperty(field)) {
              alert(`Field: ${field}, Error: ${backendErrors[field].join(', ')}`);
            }
          }
        } else {
          alert('Signup failed. Please try again!');
        }
      }
    });
  }
  closePopup(): void {
    this.popupVisible = false;
  }
}
