import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PostService {
  constructor(private http: HttpClient) {}

  getPosts(): Observable<any[]> {
   
    return of([
      {
        news_title: "JS Quick Tips",
        contents: "Some helpful JS tips...",
        mediaUrl: "assets/js.jpg",
        created_time: "2025-01-28T00:00:00Z"
      },
      {
        news_title: "Azure SQL Server",
        contents: "Setting up SQL on Azure...",
        mediaUrl: "assets/sql.jpg",
        created_time: "2025-02-10T00:00:00Z"
      }
    ]);
  }
}
