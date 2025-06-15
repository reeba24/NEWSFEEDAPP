import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {
  @Input() newsList: TileData[] = [];
   @Input() newsItem: TileData | null = null;
  @Output() tileClicked = new EventEmitter<TileData>(); 
  ngOnInit(): void {
    console.log('ForyouComponent received newsList:', this.newsList);
  }

  selecttile(news: TileData): void {
    this.tileClicked.emit(news);  
  }
}
