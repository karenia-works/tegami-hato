import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-personal-setting-page",
  templateUrl: "./personal-setting-page.component.html",
  styleUrls: ["./personal-setting-page.component.styl"]
})
export class PersonalSettingPageComponent implements OnInit {
  email;
  changedEmail;
  name;
  changedName;

  constructor() {}

  ngOnInit(): void {
    this.changedEmail = this.email = "tom@example.com";
    this.changedName = this.name = "我是一只鸽子，咕咕咕";
  }

  changeEmail() {
    if (this.changedEmail != "" && this.changedEmail != this.email) {
      var r = confirm(
        "是否将邮箱“" + this.email + "”更换为“" + this.changedEmail + "”？"
      );
      if (r == true) this.email = this.changedEmail;
    }
  }

  changeName() {
    if (this.changedName != "" && this.changedName != this.name) {
      var r = confirm(
        "是否将昵称“" + this.name + "”更改为“" + this.changedName + "”？"
      );
      if (r == true) this.name = this.changedName;
    }
  }
}
