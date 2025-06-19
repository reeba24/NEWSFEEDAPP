import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CommentModel {
  first_name:string;
last_name: string;
  comment_id: number;
  news_id: number;
  u_id: number;
  comments: string;
  created_time: string;
}

export interface TileData {
  news_id: number;
  news_title: string;
  contents: string;
  imageBase64?: string;
  first_name: string;
  last_name: string;
  likes: number;
  unlikes: number;
  created_time: string;
  comments: CommentModel[];
  u_id: number;
  isFollowed?: boolean;
  hasLiked?: boolean;
  hasUnliked?: boolean;
  hasSaved?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NewsService {
  private baseUrl = 'https://localhost:7077/api';  

  constructor(private http: HttpClient) {}

  getAllNews(userId: number): Observable<TileData[]> {
    return this.http.get<TileData[]>(`${this.baseUrl}/News/all?userId=${userId}`);
  }

  getFollowedNews(userId: number): Observable<TileData[]> {
    return this.http.get<TileData[]>(`${this.baseUrl}/Following/${userId}`);  
  }
  getFeedStatus(userId: number): Observable<any> {
  return this.http.get(`${this.baseUrl}/Feed/status?userId=${userId}`);
}
getTrendingNews(): Observable<TileData[]> {
  return this.http.get<TileData[]>('https://localhost:7077/api/TrendingNews');
}
getSearchResults(keyword: string): Observable<TileData[]> {
  return this.http.get<TileData[]>(`${this.baseUrl}/Search/${encodeURIComponent(keyword)}`);
}




}
