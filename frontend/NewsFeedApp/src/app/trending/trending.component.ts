import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { NewsService, TileData } from '../services/news.service';
import { LikeService } from '../services/like.service';
import { SavedService } from '../services/saved.service';

@Component({
  selector: 'app-trending',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trending.component.html',
  styleUrls: ['./trending.component.css'],
  providers: [NewsService, LikeService, SavedService]
})
export class TrendingComponent implements OnInit {
  trendingNews: (TileData & { hasLiked?: boolean; hasUnliked?: boolean; hasSaved?: boolean })[] = [];
  @Output() tileClicked = new EventEmitter<TileData>();

  u_id: number = parseInt(localStorage.getItem('userId') || '0');

  constructor(
    private newsService: NewsService,
    private likeService: LikeService,
    private savedService: SavedService
  ) {}

  ngOnInit(): void {
    this.newsService.getTrendingNews().subscribe({
      next: (data: TileData[]) => {
        this.trendingNews = data.map(news => ({
          ...news,
          hasLiked: false,
          hasUnliked: false,
          hasSaved: false
        }));

        this.trendingNews.forEach(news => {
          this.likeService.getFullStatus(news.news_id, this.u_id).subscribe({
            next: status => {
              news.hasLiked = status.hasLiked;
              news.hasUnliked = status.hasUnliked;
              news.hasSaved = status.hasSaved;
            },
            error: err => console.error('Full status error:', err)
          });
        });
      },
      error: (err: HttpErrorResponse) => {
        console.error('Error loading trending news:', err);
      }
    });
  }

  selecttile(news: TileData): void {
    this.tileClicked.emit(news);
  }

  like(news: TileData): void {
    if (news.hasLiked) return;

    this.likeService.performAction(news.news_id, this.u_id, 'like').subscribe({
      next: () => {
        news.likes += 1;
        if (news.hasUnliked) news.unlikes = Math.max(news.unlikes - 1, 0);
        news.hasLiked = true;
        news.hasUnliked = false;
      },
      error: err => console.error(err)
    });
  }

  unlike(news: TileData): void {
    if (news.hasUnliked) return;

    this.likeService.performAction(news.news_id, this.u_id, 'unlike').subscribe({
      next: () => {
        news.unlikes += 1;
        if (news.hasLiked) news.likes = Math.max(news.likes - 1, 0);
        news.hasUnliked = true;
        news.hasLiked = false;
      },
      error: err => console.error(err)
    });
  }

  save(news: TileData): void {
    if (news.hasSaved) return;

    this.savedService.save(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = true;
        alert('Saved!');
      },
      error: err => console.error(err)
    });
  }

  unsave(news: TileData): void {
    if (!news.hasSaved) return;

    this.savedService.unsave(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = false;
        alert('Unsaved!');
      },
      error: err => console.error(err)
    });
  }
}
