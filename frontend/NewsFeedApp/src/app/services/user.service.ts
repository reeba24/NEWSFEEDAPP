import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private baseUrl = 'https://localhost:7077/api/SignUp';  

  constructor(private http: HttpClient) {}

  signup(data: any): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post(`${this.baseUrl}/signup`, data, { headers });
  }

  login(data: any): Observable<any> {
  return this.http.post('https://localhost:7077/api/login/signin', data, { responseType: 'json' });
}

}
