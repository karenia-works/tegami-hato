import { Component, OnInit } from '@angular/core';
import { followList } from "../../sample/followList";

@Component({
  selector: 'app-follow-page',
  templateUrl: './follow-page.component.html',
  styleUrls: ['./follow-page.component.styl']
})
export class FollowPageComponent implements OnInit {
  followList = followList;

  constructor() { }

  ngOnInit() {
  }

}
