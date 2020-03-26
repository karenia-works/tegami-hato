import { Component, OnInit, OnChanges } from "@angular/core";
import { UserService } from "../../services/user.service";

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.component.html",
  styleUrls: ["./navbar.component.styl"]
})
export class NavbarComponent implements OnInit {
  get isloggedIn() {
    return this.userService.loggedIn;
  }
  get userName() {
    return this.userService.userAccount.email;
  }

  constructor(private userService: UserService) {}

  ngOnInit() {
    // this.isloggedIn = this.userService.getLogin();
    // this.userName = this.userService.getEmail();
  }
}
