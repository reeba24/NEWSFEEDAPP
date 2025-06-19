import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
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
  feedStatus: 'ready' | 'noprefs' | 'nofollowed' | 'nonews' = 'ready';
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
 
  userId: number = 0;

  constructor(
    private router: Router,
    private newsService: NewsService,
    private http: HttpClient 
  ) {}

  ngOnInit(): void {
    console.log('DashboardComponent initialized');
    this.userId = parseInt(localStorage.getItem('userId') || '0');

    this.newsService.getFeedStatus(this.userId).subscribe({
      next: (res: any) => {
        this.feedStatus = res.status;
        console.log('Feed Status:', this.feedStatus);

        if (this.feedStatus === 'ready') {
          this.newsService.getAllNews(this.userId).subscribe({
            next: (data) => {
              this.newsList = [...data];
              this.filteredNewsList = [...data];
              console.log('News loaded:', this.newsList.length);
            },
            error: (err) => console.error('Failed to load news', err)
          });
        }
      },
      error: (err) => {
        console.error('Feed status check failed', err);
      }
    });
  }

  onSearch(): void {
  const searchTerm = this.searchText.trim();
  if (searchTerm) {
    this.newsService.getSearchResults(searchTerm).subscribe({
      next: data => {
        this.searchResults = data;
        this.isSearching = true;
      },
      error: err => console.error('Search failed:', err)
    });
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

  logout() {
  
  this.showDropdown = false;
  this.logoutConfirmationVisible = true;
  
}

  confirmLogout(): void {
  this.http.post('https://localhost:7077/api/signout', {}).subscribe(() => {
    localStorage.clear();
    this.logoutConfirmationVisible = false;
    this.router.navigate(['/signin']); 
  });
}

  cancelLogout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
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
