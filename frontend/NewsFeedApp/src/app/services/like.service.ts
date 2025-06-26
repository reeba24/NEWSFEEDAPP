import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LikeService {
  private baseUrl = 'https://localhost:7077/api/LikeUnlike';

  constructor(private http: HttpClient) {}

  performAction(news_id: number, u_id: number, action: 'like' | 'unlike') {
    return this.http.post(`${this.baseUrl}/action`, { news_id, u_id, action });
  }

  getFullStatus(news_id: number, u_id: number): Observable<{ hasLiked: boolean, hasUnliked: boolean, hasSaved: boolean }> {
    return this.http.get<{ hasLiked: boolean, hasUnliked: boolean, hasSaved: boolean }>(
      `${this.baseUrl}/fullstatus?news_id=${news_id}&u_id=${u_id}`
    );
  }
  
}
