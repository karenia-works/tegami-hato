import { Component, OnInit } from "@angular/core";
import { channelMessages, channel } from "../../assets/sampleData";
import { decodeTime } from 'ulid'

@Component({
  selector: "app-group-page",
  templateUrl: "./group-page.component.html",
  styleUrls: ["./group-page.component.styl"]
})
export class GroupPageComponent implements OnInit {
  groupName;
  followNum;
  showThis;
  showNew;
  channelMessages = channelMessages;
  listLength = channelMessages.length;
  channel = channel;
  constructor() {}

  ngOnInit() {
    this.groupName = "Beihang";
    this.followNum = 125;
    this.showThis = -1;
    this.showNew = false;
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

  clickNew(){
    this.showNew = true;
  }

  ulidTime(id: string): number {
    return decodeTime(id);
  }
}
