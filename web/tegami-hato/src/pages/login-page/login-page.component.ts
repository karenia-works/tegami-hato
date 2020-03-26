import { Component, OnInit } from "@angular/core";
import { FormBuilder } from "@angular/forms";
import { UserService } from "../../services/user.service";
import { Router, ActivatedRoute } from "@angular/router";

@Component({
  templateUrl: "./login-page.component.html",
  styleUrls: ["./login-page.component.styl"]
})
export class LoginPageComponent implements OnInit {
  loginForm;
  returnTo?: string;

  constructor(
    private userService: UserService,
    private formBuilder: FormBuilder,
    private router: Router,
    private activeRoute: ActivatedRoute
  ) {
    this.loginForm = this.formBuilder.group({
      email: "",
      vertiCode: ""
    });
  }

  ngOnInit(): void {
    this.activeRoute.queryParamMap.subscribe({
      next: params => {
        this.returnTo = params.has("returnTo")
          ? params.get("returnTo")
          : undefined;
      }
    });
  }

  async vertiCode(data) {
    this.userService.verification_code(data.email);
    alert("验证码已发送至邮箱：" + data.email);
  }

  async login(data) {
    let result = await this.userService.login(
      data.email,
      data.vertiCode.trim().toUpperCase()
    );

    alert("logged in!");

    if (this.returnTo !== undefined) {
      this.router.navigate([this.returnTo]);
    } else {
      this.router.navigate(["/follow"]);
    }
  }
}
