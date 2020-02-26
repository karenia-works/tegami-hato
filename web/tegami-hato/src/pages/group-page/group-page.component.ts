import { Component, OnInit } from '@angular/core';
import { channelMessages, channel } from '../../assets/sampleData';

@Component({
  selector: 'app-group-page',
  templateUrl: './group-page.component.html',
  styleUrls: ['./group-page.component.styl']
})
export class GroupPageComponent implements OnInit {
  showNew = false;
  channelMessages = channelMessages;
  channel = channel;

  constructor() {}

  ngOnInit() {
  }
}
