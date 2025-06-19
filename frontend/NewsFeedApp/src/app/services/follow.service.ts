import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable()
export class FollowService {
  private apiUrl = 'https://localhost:7077/api/Follow';

  constructor(private http: HttpClient) {}

  followUser(followedUid: number, followedByUid: number) {
    const payload = {
      followedUid: followedUid,
      followedByUid: followedByUid
    };
    console.log('Follow request:', payload);
    return this.http.post(this.apiUrl, payload);
  }
}
