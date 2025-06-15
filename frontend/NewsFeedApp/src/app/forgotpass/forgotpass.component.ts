import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-forgotpass',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './forgotpass.component.html',
  styleUrls: ['./forgotpass.component.css']
})
export class ForgotpassComponent {
  email: string = '';
  password: string = '';
  password1: string = '';
  showPassword: boolean = false;
  popupVisible: boolean = false;
  message: string = '';
  error: string = '';

  constructor(private http: HttpClient) {}

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  openPopup(): void {
    this.message = '';
    this.error = '';

    if (this.password !== this.password1) {
      this.error = "Passwords do not match.";
      return;
    }

    const payload = {
      email: this.email,
      newPassword: this.password
    };

    this.http.post('https://localhost:7077/api/resetpassword', payload, { responseType: 'text' })
      .subscribe({
        next: (res) => {
          this.message = res;
          this.popupVisible = true;
        },
        error: (err) => {
          this.error = err.error || "Password reset failed.";
        }
      });
  }

  closePopup(): void {
    this.popupVisible = false;
    this.email = '';
    this.password = '';
    this.password1 = '';
  }
}
