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
  InvitationLink = InvitationLink;
  changedName;
  changedUserName;
  changePublic;
  constructor() {}

  ngOnInit() {
    this.changedName = channel.channelTitle;
    this.changedUserName = channel.channelUsername;
    this.changePublic = channel.isPublic;
    this.shareLink = "";
  }

  save() {
    if (this.changedName != "" && this.changedUserName != "") {
      var r = confirm(
        "将频道名称修改为：" +
          this.changedName +
          "\n将频道名称修改为：" +
          this.changedUserName +
          "\n将是否公开修改为：" +
          this.changePublic +
          "\n确认？"
      );
      if (r == true) {
        channel.channelTitle = this.changedName;
        channel.channelUsername = this.changedUserName;
        channel.isPublic = this.changePublic;
      }
    }
  }

  deleteLink(index, linkInfo) {
    var r = confirm(
      "是否删除链接hato/" +
        linkInfo.linkId +
        "？\n此链接有效期至" +
        linkInfo.expires
    );
    if (r == true) {
      InvitationLink.splice(index, 1);
      alert("链接已删除。");
    }
  }

  deleteUser(index, userInfo) {
    var r = confirm(
      "是否删除群组成员" + userInfo.name + "？\n邮箱：" + userInfo.email
    );
    if (r == true) {
      userList.splice(index, 1);
      alert(userInfo.name + "已删除。");
    }
  }
}
