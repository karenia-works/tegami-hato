import { Component, OnInit } from '@angular/core';
import { decodeTime } from 'ulid';
import { recentMessages } from '../../assets/sampleData';
import { MessageService } from '../../services/message/message.service';
import * as moment from 'moment/moment';
import { RecentMessageViewItem } from 'src/models/message';
// import {ulid} from 'ulid';

@Component({
  selector: 'app-follow-page',
  templateUrl: './follow-page.component.html',
  styleUrls: ['./follow-page.component.styl']
})

export class FollowPageComponent implements OnInit {
  // channels: = recentMessages;

  channels: RecentMessageViewItem[];

  getTime(msgId: string) {
    const time = moment(decodeTime(msgId));
    if (moment().diff(time, 'days') < 3) {
      return time.fromNow();
    } else if (time.isAfter(moment().startOf('year'))) {
      return time.format('M/D');
    } else {
      return time.format('l');
    }
  }

  constructor(
    private msgService: MessageService
  ) { }

  ngOnInit() {
    moment.locale('zh-cn');
    this.getRecentMessages();
    // console.log(ulid(new Date('2020/02/19 23:47:02').getTime()));
  }

  getRecentMessages() {
    this.msgService.getRecentMessages()
      .subscribe((data: RecentMessageViewItem[]) => this.channels = data);
  }
}
