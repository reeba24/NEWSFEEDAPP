import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private baseUrl = 'https://localhost:7077/api/Comments';

  constructor(private http: HttpClient) {}

  addComment(commentData: any): Observable<any> {
    return this.http.post(this.baseUrl, commentData);
  }

  getCommentsByNewsId(newsId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/news/${newsId}`);
  }
}
