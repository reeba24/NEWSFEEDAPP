import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';
import { LikeService } from '../services/like.service';
import { SavedService } from '../services/saved.service';

@Component({
  selector: 'app-foryou',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './foryou.component.html',
  styleUrls: ['./foryou.component.css']
})
export class ForyouComponent implements OnInit {
  @Input() newsList: TileData[] = [];
  @Input() newsItem: TileData | null = null;
  @Output() tileClicked = new EventEmitter<TileData>();

  u_id: number = parseInt(localStorage.getItem('userId') || '0');

  constructor(private likeService: LikeService, private savedService: SavedService) {}

  ngOnInit(): void {
    this.newsList.forEach(news => {
      this.likeService.getStatus(news.news_id, this.u_id).subscribe({
        next: (res: any) => {
          news.hasLiked = res.hasLiked;
          news.hasUnliked = res.hasUnliked;
        },
        error: err => console.error('Like status fetch failed', err)
      });

      this.savedService.getStatus(news.news_id, this.u_id).subscribe({
        next: (res: any) => {
          news.hasSaved = res.hasSaved;
        },
        error: err => console.error('Save status fetch failed', err)
      });
    });
  }

  selecttile(news: TileData): void {
    this.tileClicked.emit(news);
  }

  like(news: TileData): void {
    if (news.hasLiked) return;

    this.likeService.like(news.news_id, this.u_id).subscribe({
      next: () => {
        news.likes += 1;
        news.hasLiked = true;
        news.hasUnliked = false;
      },
      error: err => console.error(err)
    });
  }

  unlike(news: TileData): void {
    this.likeService.unlike(news.news_id, this.u_id).subscribe({
      next: () => {
        news.unlikes += 1;
        if (news.hasLiked) {
          news.likes = Math.max(news.likes - 1, 0);
        }
        news.hasLiked = false;
        news.hasUnliked = true;
      },
      error: err => console.error(err)
    });
  }

  save(news: TileData): void {
    if (news.hasSaved) return;

    this.savedService.save(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = true;
      },
      error: err => console.error(err)
    });
  }
}
