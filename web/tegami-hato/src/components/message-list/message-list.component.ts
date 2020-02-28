import { Component, OnInit } from '@angular/core';
import { Input } from '@angular/core';
import { decodeTime } from 'ulid';
import * as moment from 'moment/moment';

@Component({
  selector: 'app-message-list',
  templateUrl: './message-list.component.html',
  styleUrls: ['./message-list.component.styl']
})
export class MessageListComponent implements OnInit {
  @Input() list;
  @Input() showChannel = false;

  showThis = 0;
  constructor() { }

  ngOnInit(): void {
    moment.locale('zh-cn');
  }

  clickToOpen(index): void {

    if (this.showThis == index) {
      this.showThis = -1;
    } else {
      this.showThis = index;
    }
  }

  ulidTime(id: string): number {
    return decodeTime(id);
  }

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
}
