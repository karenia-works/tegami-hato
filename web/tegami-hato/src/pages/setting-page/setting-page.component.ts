import { Component, OnInit } from "@angular/core";
import { userList } from "../../sample/followList";
import { channel, InvitationLink } from "../../assets/sampleData";

@Component({
  selector: "app-setting-page",
  templateUrl: "./setting-page.component.html",
  styleUrls: ["./setting-page.component.styl"]
})
export class SettingPageComponent implements OnInit {
  groupName;
  groupUserName;
  isPublic;
  shareLink;
  userList = userList;
  channel = channel;
  InvitationLink= InvitationLink;
  constructor() {}

  ngOnInit() {
    this.groupName = "北航软院2017级通知";
    this.groupUserName = "Beihang1721";
    this.isPublic = true;
    this.shareLink = "";
  }
}
