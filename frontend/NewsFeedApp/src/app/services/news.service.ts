import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TileData {
  news_id: number;
  news_title: string;
  contents: string;
  imageBase64?: string | null;
  first_name: string;
  last_name: string;
  likes: number;
  unlikes: number;
  created_time: string; 
}

@Injectable({
  providedIn: 'root'
})
export class NewsService {
  private apiUrl = 'https://localhost:7077/api/news/all'; 

  constructor(private http: HttpClient) {}

  getAllNews(): Observable<TileData[]> {
    return this.http.get<TileData[]>(this.apiUrl);
  }
}
