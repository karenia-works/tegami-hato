import { Component, OnInit } from "@angular/core";

@Component({
  selector: "app-share-page",
  templateUrl: "./share-page.component.html",
  styleUrls: ["./share-page.component.styl"]
})
export class SharePageComponent implements OnInit {
  length;
  Receive;
  Send;
  Edit;
  UserManage;
  EditInfo;
  Authority;
  link;

  constructor() {}

  ngOnInit(): void {
    this.length = 0;
    this.Receive = this.Send = this.Edit = this.UserManage = this.EditInfo = 0;
    this.link = "hato/AXBYUom4Nf2GDcMyaH3pw";
  }

  create() {
    if (this.length != 0) {
      this.Authority =
        this.Receive + this.Send + this.Edit + this.UserManage + this.EditInfo;
      if(this.Authority != 0){
        alert("已生成链接：" + this.link);
      } else alert("未选择有效权限！");
    } else alert("未选择有效时长！");
  }
}
