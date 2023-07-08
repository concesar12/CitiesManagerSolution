import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RegisterUser } from '../models/register-user';

const API_BASE_URL: string = "https://localhost:7163/api/v1/account/";

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private httpClient: HttpClient) {
  }
  //We are having Observable Register user because that is what we receive in our controller on httpost
  public postRegister(registerUser: RegisterUser): Observable<RegisterUser> {
    return this.httpClient.post<RegisterUser>(`${API_BASE_URL}register`, registerUser);
  }
}
