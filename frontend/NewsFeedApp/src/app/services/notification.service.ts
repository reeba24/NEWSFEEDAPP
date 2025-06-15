import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private apiUrl = 'https://localhost:7077/api/Notification'; 

  constructor(private http: HttpClient) {}

  getNotifications(userId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/GetNotifications/${userId}`);
  }

  markAsRead(notificationId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/MarkAsRead/${notificationId}`, {});
  }

  clearAll(userId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/ClearAll/${userId}`, {});
  }
}
