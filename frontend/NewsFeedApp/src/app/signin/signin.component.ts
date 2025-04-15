import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service'; 

@Component({
  selector: 'app-signin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.css']
})
export class SigninComponent {
  showPassword: boolean = false;
  popupVisible: boolean = false;
  user = '';
  password = '';
  constructor(private userService: UserService) {}

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  openPopup(): void {
    const loginData = { email: this.user, password: this.password };
    this.userService.login(loginData).subscribe({
      next: () => {
        this.popupVisible = true;  
      },
      error: () => {
        alert('Invalid username or password.');
      }
    });
  }
  closePopup(): void {
    this.popupVisible = false;
  }
}
