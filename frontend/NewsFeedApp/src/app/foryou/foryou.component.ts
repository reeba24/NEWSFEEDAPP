import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';


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

  ngOnInit(): void {
    console.log('ForyouComponent received newsList:', this.newsList);
  }

  selecttile(news: TileData): void {
    this.tileClicked.emit(news);  
  }
}
