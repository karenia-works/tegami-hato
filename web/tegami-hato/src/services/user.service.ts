import { HttpClient, HttpInterceptor } from '@angular/common/http';
import { HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, config, Subject } from 'rxjs';
import 'rxjs/operators';
import { apiConfig } from 'src/environments/backend-config';
import { multicast } from 'rxjs/operators';
import { LoginResult, TokenContext, UserAccount } from 'src/models/account';
import { environment } from 'src/environments/environment';
import { ApiResult } from 'src/models/result';
import qs from 'qs';
import JwtDecode from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class UserService extends Subject<UserAccount | undefined>
  implements HttpInterceptor {

  constructor(private httpClient: HttpClient) {
    super();
    this.next(undefined);
    this.loadLoginData();
  }

  public loggedIn = false;
  loginResult?: LoginResult;
  userAccount?: UserAccount;

  /**
   * Intercepts any outgoing HTTP request toward backend and add access token to them
   * @param req Request
   */
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    if (this.loggedIn) {
      req.headers.append(
        'Authorization',
        `Bearer ${this.loginResult.access_token}`
      );
    }
    return next.handle(req);
  }

  saveLoginData() {
    if (this.loggedIn) {
      window.localStorage.setItem('login', JSON.stringify(this.loginResult));
      window.localStorage.setItem('user', JSON.stringify(this.userAccount));
    }
  }

  async loadLoginData() {
    let login = window.localStorage.getItem('login');
    let user = window.localStorage.getItem('user');
    if (login !== null && user !== null) {
      this.loginResult = JSON.parse(login);
      let username = JwtDecode(this.loginResult.access_token)['sub'];
      this.loggedIn = true;
      this.userAccount = JSON.parse(user);
      this.next(this.userAccount);
    }
  }

  clearLoginData() {
    window.localStorage.removeItem('login');
  }

  async verification_code(useremail: string) {
    try {
      return this.httpClient
        .post(
          environment.endpoint + apiConfig.endpoints.account.verification,
          undefined,
          { params: {email: useremail} },
        )
        .toPromise();
    } catch (e) {
      throw new Error('Failed to sent code. Reason: ' + JSON.stringify(e.error));
    }
  }

  async login(username: string, password: string): Promise<LoginResult> {
    let ctx: TokenContext = {
      client_id: 'WebClient',
      client_secret: 'WebClientPublic',
      grant_type: 'password',
      username,
      password,
      scope: 'api',
    };
    try {
      let result = await this.httpClient
        .post<LoginResult>(
          environment.endpoint + apiConfig.endpoints.account.login,
          qs.stringify(ctx),
          {
            headers: {
              'Content-Type': 'application/x-www-form-urlencoded',
            },
          }
        )
        .toPromise();
      this.loginResult = result;
      this.loggedIn = true;
      let username = JwtDecode(this.loginResult.access_token)['sub'];
      let account = await this.httpClient
        .get<ApiResult<UserAccount>>(
          environment.endpoint + apiConfig.endpoints.account.info.current
        )
        .toPromise();
      this.userAccount = account.data;
      this.saveLoginData();
      return result;
    } catch (e) {
      this.loggedIn = false;
      throw new Error('Failed to login. Reason: ' + JSON.stringify(e.error));
    }
  }

  logout() {
    this.loggedIn = false;
    this.loginResult = undefined;
    this.clearLoginData();
  }
}

export interface UserInfo {
  username: string;
}


// import { Injectable } from '@angular/core';

// @Injectable({
//   providedIn: 'root'
// })
// export class UserService {
//   isLogin: boolean = false;
//   email: string = '';

//   constructor() { }

//   login(email: string) {
//     this.isLogin = true;
//     this.email = email;
//     console.log('login successfully')
//   }

//   getEmail(): string {
//     return this.email;
//   }

//   getLogin(): boolean {
//     return this.isLogin;
//   }

// }
