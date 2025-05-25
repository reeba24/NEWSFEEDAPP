import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Router, RouterModule } from '@angular/router';
import { ForyouComponent } from '../foryou/foryou.component';
import { TrendingComponent } from '../trending/trending.component';
import { NewsService, TileData } from '../services/news.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ForyouComponent, TrendingComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  
  newsList: TileData[] = [];
  showDropdown = false;
  logoutConfirmationVisible = false;

  activeTab: 'forYou' | 'trending' = 'forYou';

  constructor(
     
    private router: Router, 
    private newsService: NewsService
  ) {}

  ngOnInit(): void {
    console.log('DashboardComponent initialized');

    

    this.newsService.getAllNews().subscribe({
      next: (data) => {
        this.newsList = [...data];
        console.log('News loaded:', this.newsList.length);
      },
      error: (err) => console.error('Failed to load news', err)
    });
  }

  openforyou(): void {
    this.activeTab = 'forYou';
  }

  opentrending(): void {
    this.activeTab = 'trending';
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

  homeclick(): void {
    this.activeTab = 'forYou';
  }
}
