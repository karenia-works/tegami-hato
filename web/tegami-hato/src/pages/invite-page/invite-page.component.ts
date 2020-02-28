import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-invite-page",
  templateUrl: "./invite-page.component.html",
  styleUrls: ["./invite-page.component.styl"]
})
export class InvitePageComponent implements OnInit {
  userName;
  email;
  groupName;

  constructor() {}

  ngOnInit(): void {
    this.userName = "费心怡";
    this.email = "df@buaa.edu.cn";
    this.groupName = "北航软院2017级通知"
  }
}
