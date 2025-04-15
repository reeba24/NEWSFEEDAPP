import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-forgotpass',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './forgotpass.component.html',
  styleUrls: ['./forgotpass.component.css']
})
export class ForgotpassComponent {
  showPassword: boolean = false;
  user: any = {};
  password: string = '';
  password1: string = '';
  popupVisible: boolean = false;

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  openPopup(): void {
    if (this.password === this.password1) {
      this.popupVisible = true;
    } else {
      alert("Passwords do not match.");
    }
  }

  closePopup(): void {
    this.popupVisible = false;
  }
}
