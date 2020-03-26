import { Component, OnInit } from "@angular/core";
import { UserService } from "src/services/user.service";
import { Router } from "@angular/router";

@Component({
  selector: "app-invite-page",
  templateUrl: "./invite-page.component.html",
  styleUrls: ["./invite-page.component.styl"]
})
export class InvitePageComponent implements OnInit {
  userName;
  email;
  groupName;

  constructor(private userService: UserService, private router: Router) {}

  ngOnInit(): void {
    this.userName = "费心怡";
    this.email = "df@buaa.edu.cn";
    this.groupName = "1721 大班通知";
  }

  acceptInvitation() {
    if (this.userService.loggedIn) {
      this.router.navigate(["/channel", "01E1K53HW9MXR3D8MTR205MHFZ"]);
    } else {
      this.router.navigate(["/login"], {
        queryParams: {
          returnTo: "/channel/01E1K53HW9MXR3D8MTR205MHFZ"
        }
      });
    }
  }
}
