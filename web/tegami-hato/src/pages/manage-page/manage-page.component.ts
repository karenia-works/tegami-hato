import { Component, OnInit } from "@angular/core";
import { userList,linkList } from "../../sample/followList";

@Component({
  selector: "app-manage-page",
  templateUrl: "./manage-page.component.html",
  styleUrls: ["./manage-page.component.styl"]
})
export class ManagePageComponent implements OnInit {
  groupName;
  groupUserName;
  isPublic;
  shareLink;
  userList = userList;
  linkList = linkList;
  constructor() {}

  ngOnInit() {
    this.groupName = "北航软院2017级通知";
    this.groupUserName = "Beihang1721";
    this.isPublic = true;
    this.shareLink = "";
  }
}
