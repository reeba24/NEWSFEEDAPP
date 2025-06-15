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

  constructor(private likeService: LikeService, private savedService: SavedService) { }

  u_id: number = parseInt(localStorage.getItem('userId') || '0');

  ngOnInit(): void {
    console.log('ForyouComponent received newsList:', this.newsList);
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
      next: () => alert('Saved!'),
      error: err => console.error(err)
    });
  }
}
