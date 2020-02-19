import { Component, OnInit } from "@angular/core";
import { NoticeList } from "../../sample/followList";

@Component({
  selector: "app-group-page",
  templateUrl: "./group-page.component.html",
  styleUrls: ["./group-page.component.styl"]
})
export class GroupPageComponent implements OnInit {
  groupName;
  followNum;
  showThis;
  NoticeList = NoticeList;
  listLength = NoticeList.length;
  constructor() {}

  ngOnInit() {
    this.groupName = "Beihang";
    this.followNum = 125;
    this.showThis = -1;
  }

  clickToOpen(index) {
    if (this.showThis == -1) {
      this.showThis = index;
      return;
    }
    if (this.showThis == index) {
      this.showThis = -1;
      return;
    } else {
      this.showThis = index;
      return;
    }
  }
}
