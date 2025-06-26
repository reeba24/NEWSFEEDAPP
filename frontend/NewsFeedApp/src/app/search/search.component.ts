import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';
import { LikeService } from '../services/like.service';
import { SavedService } from '../services/saved.service';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnChanges {
  @Input() newsList: TileData[] = [];
  @Output() tileClicked = new EventEmitter<TileData>();

  u_id: number = parseInt(localStorage.getItem('userId') || '0');

  constructor(
    private likeService: LikeService,
    private savedService: SavedService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['newsList'] && this.newsList?.length > 0) {
      this.newsList.forEach(news => {
        this.likeService.getFullStatus(news.news_id, this.u_id).subscribe({
          next: res => {
            news.hasLiked = res.hasLiked;
            news.hasUnliked = res.hasUnliked;
            news.hasSaved = res.hasSaved;
          },
          error: err => console.error('Status fetch failed:', err)
        });
      });
    }
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

  unsave(news: TileData): void {
    if (!news.hasSaved) return;

    this.savedService.unsave(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = false;
      },
      error: err => console.error('Error unsaving news:', err)
    });
  }
}
