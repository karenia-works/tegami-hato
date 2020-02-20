import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  isLogin: boolean = false;
  email: string = '';

  constructor() { }

  login(email: string) {
    this.isLogin = true;
    this.email = email;
    console.log('login successfully')
  }

  getEmail(): string {
    return this.email;
  }

  getLogin(): boolean {
    return this.isLogin;
  }

}
