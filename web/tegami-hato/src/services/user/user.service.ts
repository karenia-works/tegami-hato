import { HttpClient, HttpInterceptor } from "@angular/common/http";
import { HttpRequest, HttpHandler, HttpEvent } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, config, Subject } from "rxjs";
import "rxjs/operators";
import { multicast } from "rxjs/operators";
import { environment } from "src/environments/environment";
import { apiConfig } from "src/environments/backend-config";
import { LoginResult, TokenContext, UserAccount } from "src/models/account";
import { ApiResult } from "src/models/result";
import qs from "qs";
import JwtDecode from "jwt-decode";

@Injectable({ providedIn: "root" })
export class UserInjector implements HttpInterceptor {
  private loggedIn = false;
  private loginResult?: LoginResult = undefined;

  private watching = false;

  /**
   * Intercepts any outgoing HTTP request toward backend and add access token to them
   * @param req Request
   */
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    console.log(this);
    if (this.loggedIn) {
      req.headers.append(
        "Authorization",
        `Bearer ${this.loginResult.access_token}`
      );
    }
    return next.handle(req);
  }

  public watch(tgt: Observable<LoginResult | undefined>) {
    if (this.watching) {
      throw new Error("Already watching another UserAccount source");
    }
    tgt.subscribe({
      next: val => {
        console.log("got new loginresult", val);
        if (val === undefined) {
          this.loggedIn = false;
          this.loginResult = undefined;
        } else {
          this.loggedIn = true;
          this.loginResult = val;
        }
      }
    });
    this.watching = true;
  }
}

@Injectable({ providedIn: "root" })
export class UserService extends Subject<UserAccount | undefined>
  implements HttpInterceptor {
  constructor(
    private httpClient: HttpClient // private userInjector: UserInjector
  ) {
    super();
    this.next(undefined);
    this.loadLoginData();
    // this.userInjector.watch(this.loginResultAnnouncer);
  }

  public loggedIn = false;
  loginResult?: LoginResult;
  userAccount?: UserAccount;

  loginResultAnnouncer = new Subject<LoginResult | undefined>();

  /**
   * Intercepts any outgoing HTTP request toward backend and add access token to them
   * @param req Request
   */
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    console.log(this);
    if (this.loggedIn) {
      req.headers.append(
        "Authorization",
        `Bearer ${this.loginResult.access_token}`
      );
    }
    return next.handle(req);
  }

  saveLoginData() {
    if (this.loggedIn) {
      window.localStorage.setItem("login", JSON.stringify(this.loginResult));
      window.localStorage.setItem("user", JSON.stringify(this.userAccount));
    }
  }

  async loadLoginData() {
    let login = window.localStorage.getItem("login");
    let user = window.localStorage.getItem("user");
    if (login !== null && user !== null) {
      this.loginResult = JSON.parse(login);
      let username = JwtDecode(this.loginResult.access_token)["sub"];
      this.loggedIn = true;
      this.userAccount = JSON.parse(user);
      this.next(this.userAccount);
      this.loginResultAnnouncer.next(this.loginResult);
    }
  }

  clearLoginData() {
    window.localStorage.removeItem("login");
    this.loginResultAnnouncer.next(undefined);
  }

  async verification_code(useremail: string) {
    try {
      return this.httpClient
        .post(
          environment.endpoint + apiConfig.endpoints.account.verification,
          undefined,
          { params: { email: useremail } }
        )
        .toPromise();
    } catch (e) {
      throw new Error(
        "Failed to sent code. Reason: " + JSON.stringify(e.error)
      );
    }
  }

  async login(username: string, password: string): Promise<LoginResult> {
    let ctx: TokenContext = {
      client_id: "WebClient",
      client_secret: "WebClientPublic",
      grant_type: "password",
      username,
      password,
      scope: "api"
    };
    try {
      let result = await this.httpClient
        .post<LoginResult>(
          environment.endpoint + apiConfig.endpoints.account.login,
          qs.stringify(ctx),
          {
            headers: {
              "Content-Type": "application/x-www-form-urlencoded"
            }
          }
        )
        .toPromise();
      console.log("Got new result", result);
      this.loginResult = result;
      this.loginResultAnnouncer.next(this.loginResult);
      let username = JwtDecode(this.loginResult.access_token)["sub"];
      let account = await this.httpClient
        .get<UserAccount>(
          environment.endpoint + apiConfig.endpoints.account.info.current,
          {
            headers: {
              Authorization: `Bearer ${this.loginResult.access_token}`
            }
          }
        )
        .toPromise();
      this.userAccount = account;
      this.loggedIn = true;
      this.saveLoginData();
      return result;
    } catch (e) {
      this.loggedIn = false;

      this.loginResultAnnouncer.next(undefined);
      throw new Error("Failed to login. Reason: " + JSON.stringify(e.error));
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
