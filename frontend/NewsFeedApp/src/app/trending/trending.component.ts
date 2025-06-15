import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';

@Component({
  selector: 'app-trending',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trending.component.html',
  styleUrls: ['./trending.component.css']
})
export class TrendingComponent implements OnInit {
  allNews: any[] = [];
  trendingNews: TileData[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get<any[]>('https://localhost:7077/api/News/all')  
      .subscribe(data => {
        this.allNews = data;

        this.trendingNews = this.allNews
          .sort((a, b) => Number(b.likes) - Number(a.likes))
          .slice(0, 5); 
      });
  }
  selecttile(news: TileData): void {
    console.log('Trending tile clicked:', news);
  
}
}
