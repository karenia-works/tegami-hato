import { Component, OnInit } from '@angular/core';
import { Input } from '@angular/core';
import { decodeTime } from 'ulid';

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
}
