import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

import { ForyouComponent } from '../foryou/foryou.component';
import { TrendingComponent } from '../trending/trending.component';
import { TilenewsdetailsComponent } from '../tilenewsdetails/tilenewsdetails.component';
import { NewsService, TileData } from '../services/news.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ForyouComponent,
    TrendingComponent,
    TilenewsdetailsComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  newsList: TileData[] = [];
  selectedNewsItem: TileData | null = null;
  showDropdown = false;
  logoutConfirmationVisible = false;

  activeTab: 'forYou' | 'trending' | 'tilenewsdetails' = 'forYou';
 

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
    this.selectedNewsItem = null;
  }

  opentrending(): void {
    this.activeTab = 'trending';
    this.selectedNewsItem = null;
  }

  opentiledata(newsItem: TileData): void {
    this.selectedNewsItem = newsItem;
    this.activeTab = 'tilenewsdetails';
  }

  backToForYou(): void {
    this.activeTab = 'forYou';
    this.selectedNewsItem = null;
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
    this.selectedNewsItem = null;
  }
}
