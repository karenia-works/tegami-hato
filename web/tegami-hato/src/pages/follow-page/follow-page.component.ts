import { Component, OnInit } from '@angular/core';
import { decodeTime } from 'ulid'
import { recentMessages } from '../../assets/sampleData';
import * as moment from 'moment/moment'

@Component({
  selector: 'app-follow-page',
  templateUrl: './follow-page.component.html',
  styleUrls: ['./follow-page.component.styl']
})

export class FollowPageComponent implements OnInit {
  channels = recentMessages;

  getTime(msgId: string) {
    let time = moment(decodeTime(msgId));
    if (moment().diff(time, 'days') < 3)
      return time.fromNow();
    else if(time.isAfter(moment().startOf('year')))
      return time.format('M/D'); 
    else
      return time.format('l'); 
  }

  constructor() { }

  ngOnInit() {
    moment.locale('zh-cn');
  }

}
