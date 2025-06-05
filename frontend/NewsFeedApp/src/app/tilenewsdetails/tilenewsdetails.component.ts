import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';

@Component({
  selector: 'app-tilenewsdetails',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tilenewsdetails.component.html',
  styleUrls: ['./tilenewsdetails.component.css']
})
export class TilenewsdetailsComponent {
  @Input() newsItem: TileData | null = null;
  @Output() backClicked = new EventEmitter<void>();
  showComments = false;

  goBack(): void {
    this.backClicked.emit();
  }
  toggleComments() {
    this.showComments = !this.showComments;
  }
}
