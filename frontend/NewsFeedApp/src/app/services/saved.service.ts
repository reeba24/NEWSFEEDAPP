import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TileData } from './news.service';

@Injectable({
  providedIn: 'root'
})
export class SavedService {
  private baseUrl = 'https://localhost:7077/api/Saved';

  constructor(private http: HttpClient) {}

  save(news_id: number, u_id: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/save`, { news_id, u_id });
  }

  getSavedNews(u_id: number): Observable<TileData[]> {
    return this.http.get<TileData[]>(`${this.baseUrl}/getsavednews/${u_id}`);
  }
  unsave(news_id: number, u_id: number) {
  return this.http.post(`${this.baseUrl}/unsave`, { news_id, u_id });
}
}
