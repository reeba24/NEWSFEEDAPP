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
          this.likeService.getStatus(news.news_id, this.u_id).subscribe({
            next: status => {
              news.hasLiked = status.hasLiked;
              news.hasUnliked = status.hasUnliked;
            },
            error: err => console.error('Like status error:', err)
          });

          this.savedService.getStatus(news.news_id, this.u_id).subscribe({
            next: status => {
              news.hasSaved = status.hasSaved;
            },
            error: err => console.error('Saved status error:', err)
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

  like(news: any): void {
    this.likeService.like(news.news_id, this.u_id).subscribe({
      next: () => {
        news.likes += 1;
        news.hasLiked = true;
        news.hasUnliked = false;
        if (news.unlikes > 0) news.unlikes -= 1;
      },
      error: err => console.error(err)
    });
  }

  unlike(news: any): void {
    this.likeService.unlike(news.news_id, this.u_id).subscribe({
      next: () => {
        news.unlikes += 1;
        news.hasUnliked = true;
        news.hasLiked = false;
        if (news.likes > 0) news.likes -= 1;
      },
      error: err => console.error(err)
    });
  }

  save(news: any): void {
    this.savedService.save(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = true;
      },
      error: err => console.error(err)
    });
  }
}
