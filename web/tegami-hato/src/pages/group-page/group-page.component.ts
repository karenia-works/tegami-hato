import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-group-page',
  templateUrl: './group-page.component.html',
  styleUrls: ['./group-page.component.styl']
})
export class GroupPageComponent implements OnInit {
  groupName;
  followNum;
  constructor() { }

  ngOnInit() {
    this.groupName = 'Beihang';
    this.followNum = 125;
  }

}
