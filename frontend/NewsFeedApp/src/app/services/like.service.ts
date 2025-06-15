import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class LikeService {
  private baseUrl = 'https://localhost:7077/api/LikeUnlike';

  constructor(private http: HttpClient) {}

  like(news_id: number, u_id: number) {
    return this.http.post(`${this.baseUrl}/like`, { news_id, u_id });
  }

  unlike(news_id: number, u_id: number) {
    return this.http.post(`${this.baseUrl}/unlike`, { news_id, u_id });
  }
}
