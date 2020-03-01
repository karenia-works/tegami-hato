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

  vertiCode() {
    alert("验证码已发送");
  }

  login(data) {
    if (this.returnTo !== undefined) {
      this.router.navigate([this.returnTo]);
    } else {
      this.router.navigate(["/follow"]);
    }
  }
}
