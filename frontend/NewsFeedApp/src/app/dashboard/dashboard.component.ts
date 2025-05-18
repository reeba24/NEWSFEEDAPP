import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PostService } from '../services/post.service';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  posts: any[] = [];
  showDropdown = false;
  logoutConfirmationVisible = false;

  constructor(private postService: PostService, private router: Router) {}

  ngOnInit(): void {
    console.log('DashboardComponent initialized');
    this.postService.getPosts().subscribe(data => {
      this.posts = data;
      console.log('Posts loaded:', this.posts.length);
    });
  }

  openforyou(): void {
    console.log('Navigating to For You');
    this.router.navigate(['/foryou']);
  }

  opentrending(): void {
    console.log('Navigating to Trending');
    this.router.navigate(['/trending']);
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
    console.log('Dropdown toggled. Now:', this.showDropdown);
  }

  closeDropdown(): void {
    setTimeout(() => {
      this.showDropdown = false;
      console.log('Dropdown closed');
    }, 150);
  }

  editProfile(): void {
    console.log('Edit Profile clicked');
    this.showDropdown = false;
    this.router.navigate(['/edit-profile']);
  }

  logout(): void {
    console.log('Logout clicked');
    this.showDropdown = false;
    this.logoutConfirmationVisible = true;
    console.log('logoutConfirmationVisible set to true');
  }

  confirmLogout(): void {
    console.log('Logout confirmed');
    this.logoutConfirmationVisible = false;
    this.router.navigate(['/']);
  }

  cancelLogout(): void {
    console.log('Logout cancelled');
    this.logoutConfirmationVisible = false;
  }
}
