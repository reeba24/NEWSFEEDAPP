import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

interface FollowPayload {
  followedUid: number;
  followedByUid: number;
}

@Injectable()
export class FollowService {
  private apiUrl = 'https://localhost:7077/api/Follow';

  constructor(private http: HttpClient) {}

  followUser(followedUid: number, followedByUid: number): Observable<any> {
    const payload: FollowPayload = {
      followedUid,
      followedByUid
    };

    console.log('Sending follow/unfollow request:', payload);
    return this.http.post(this.apiUrl, payload);
  }
}
