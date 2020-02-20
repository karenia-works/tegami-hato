import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms'
import { UserService } from '../../services/user.service'
import { Router } from '@angular/router'

@Component({
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.styl']
})
export class LoginPageComponent implements OnInit {
  loginForm;

  constructor(
    private userService: UserService,
    private formBuilder: FormBuilder,
    private router: Router
  ) {
    this.loginForm = this.formBuilder.group({
      email: '',
      vertiCode: ''
    });
   }

  ngOnInit(): void {
  }

  vertiCode() {
    alert("验证码已发送");
  }

  login(data) {
    this.userService.login(data.email);
    window.alert("欢迎，" + data.email);
    this.router.navigate(['/follow']);
  }
}
