import { Component, Input, OnInit } from '@angular/core';
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

  selectedNews: TileData | null = null;

  ngOnInit(): void {
    console.log('ForyouComponent received newsList:', this.newsList);
  }

  selecttile(news: TileData): void {
    this.selectedNews = news;
  }

  clearSelection(): void {
    this.selectedNews = null;
  }
}
