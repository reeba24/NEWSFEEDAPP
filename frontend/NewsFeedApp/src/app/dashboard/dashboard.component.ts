import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EditprofileComponent } from '../editprofile/editprofile.component'; 
import { ForyouComponent } from '../foryou/foryou.component';
import { TrendingComponent } from '../trending/trending.component';
import { TilenewsdetailsComponent } from '../tilenewsdetails/tilenewsdetails.component';
import { NewsService, TileData } from '../services/news.service';
import { NewpostComponent } from '../newpost/newpost.component';
import { SearchComponent } from '../search/search.component';
import { FollowingComponent } from '../following/following.component';
import { SavedComponent } from '../saved/saved.component';
import { NotificationsComponent } from '../notifications/notifications.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ForyouComponent,
    FormsModule,
    TrendingComponent,
    TilenewsdetailsComponent,
    EditprofileComponent,
    NewpostComponent,
    SearchComponent,
    FollowingComponent,
    SavedComponent,
    NotificationsComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  newsList: TileData[] = [];
  filteredNewsList: TileData[] = [];
  searchResults: TileData[] = [];
  isSearching: boolean = false;
  selectedNewsItem: TileData | null = null;
  showDropdown = false;
  logoutConfirmationVisible = false;
  searchText: string = '';

  activeTab: 'forYou' | 'trending' | 'tilenewsdetails' | 'editprofile' | 'newpost' | 'following' | 'saved' = 'forYou';


  showNotifications: boolean = false;

  constructor(
    private router: Router,
    private newsService: NewsService
  ) {}

  ngOnInit(): void {
    console.log('DashboardComponent initialized');

    this.newsService.getAllNews().subscribe({
      next: (data) => {
        this.newsList = [...data];
        this.filteredNewsList = [...data];
        console.log('News loaded:', this.newsList.length);
      },
      error: (err) => console.error('Failed to load news', err)
    });
  }

  onSearch(): void {
    const searchTerm = this.searchText.trim().toLowerCase();
    if (searchTerm) {
      this.searchResults = this.newsList.filter(news =>
        news.news_title.toLowerCase().includes(searchTerm)
      );
      this.isSearching = true;
    } else {
      this.clearSearch(); 
    }
  }

  clearSearch(): void {
    this.searchText = '';
    this.searchResults = [];
    this.isSearching = false; 
    this.activeTab = 'forYou'; 
  }

  openforyou(): void {
    this.activeTab = 'forYou';
    this.selectedNewsItem = null;
    this.isSearching = false;
  }

  opentrending(): void {
    this.activeTab = 'trending';
    this.selectedNewsItem = null;
    this.isSearching = false;
  }

  opentiledata(newsItem: TileData): void {
    this.selectedNewsItem = newsItem;
    this.activeTab = 'tilenewsdetails';
    this.isSearching = false;
  }

  openeditprofile(): void {
    this.activeTab = 'editprofile';
    this.isSearching = false;
  }

  opennewpost(): void {
    this.activeTab = 'newpost';
    this.isSearching = false;
  }

  backToForYou(): void {
    this.activeTab = 'forYou';
    this.selectedNewsItem = null;
    this.isSearching = false;
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
    console.log('Dropdown toggled. Now:', this.showDropdown);
  }

  editprofile(): void {
    this.showDropdown = false;
    this.router.navigate(['/editprofile']);
  }

  closeDropdown(): void {
    setTimeout(() => {
      this.showDropdown = false;
      console.log('Dropdown closed');
    }, 150);
  }

  logout(): void {
    console.log('Logout clicked');
    this.showDropdown = false;
    this.logoutConfirmationVisible = true;
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
    this.isSearching = false;
  }

  followingclick(): void {
    this.activeTab = 'following';
    this.selectedNewsItem = null;
    this.isSearching = false;
  }

  savedclick(): void {
    this.activeTab = 'saved';
    this.selectedNewsItem = null;
    this.isSearching = false;
  }

  
  noticlick(): void {
    this.showNotifications = !this.showNotifications;
  }
  closeNotifications(): void {
  this.showNotifications = false;
}
}
