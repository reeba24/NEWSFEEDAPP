import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';
import { LikeService } from '../services/like.service';
import { SavedService } from '../services/saved.service';

@Component({
  selector: 'app-saved',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './saved.component.html',
  styleUrls: ['./saved.component.css']
})
export class SavedComponent implements OnInit {
  @Output() tileClicked = new EventEmitter<TileData>();

  u_id: number = parseInt(localStorage.getItem('userId') || '0');
  savedNewsList: TileData[] = [];

  constructor(
    private savedService: SavedService,
    private likeService: LikeService
  ) {}

  ngOnInit(): void {
    this.loadSavedNews();
  }

  loadSavedNews(): void {
    this.savedService.getSavedNews(this.u_id).subscribe({
      next: (data) => {
        this.savedNewsList = data;
        console.log('Loaded saved news:', this.savedNewsList);
      },
      error: (err) => console.error('Failed to load saved news:', err)
    });
  }

  selecttile(news: TileData): void {
    this.tileClicked.emit(news);
  }

  like(news: TileData): void {
    this.likeService.like(news.news_id, this.u_id).subscribe({
      next: () => {
        news.likes += 1;
      },
      error: err => console.error(err)
    });
  }

  unlike(news: TileData): void {
    this.likeService.unlike(news.news_id, this.u_id).subscribe({
      next: () => {
        news.unlikes += 1;
        if (news.likes > 0) {
          news.likes -= 1;
        }
      },
      error: err => console.error(err)
    });
  }

  save(news: TileData): void {
    this.savedService.save(news.news_id, this.u_id).subscribe({
      next: () => alert('Saved again!'),
      error: err => console.error(err)
    });
  }
}
