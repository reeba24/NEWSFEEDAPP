import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.css']
})
export class SigninComponent {
  showPassword: boolean = false;
  user = '';
  password = '';

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  login(): void {
    const loginData = { email: this.user, password: this.password };
    this.userService.login(loginData).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        alert('Invalid username or password.');
      }
    });
  }
}
