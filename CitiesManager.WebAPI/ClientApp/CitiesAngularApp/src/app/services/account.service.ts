import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RegisterUser } from '../models/register-user';
import { LoginUser } from '../models/login-user';

//HERE IS WHERE WE INTERACT WITH OUR C# API
const API_BASE_URL: string = "https://localhost:7163/api/v1/account/";

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  //To check if user is logged in
  public currentUserName: string | null = null;

  constructor(private httpClient: HttpClient) {
  }
  //We are having Observable Register user because that is what we receive in our controller on httpost
  public postRegister(registerUser: RegisterUser): Observable<any> {
    return this.httpClient.post<any>(`${API_BASE_URL}register`, registerUser);
  }

  public postLogin(loginUser: LoginUser): Observable<any> {
    return this.httpClient.post<any>(`${API_BASE_URL}login`, loginUser);
  }

  public getLogout(): Observable<string> {
    return this.httpClient.get<string>(`${API_BASE_URL}logout`);
  }


}
