import { Component, OnInit } from '@angular/core';
import { channelMessages } from '../../assets/sampleData';

@Component({
  selector: 'app-news',
  templateUrl: './news.component.html',
  styleUrls: ['./news.component.styl']
})
export class NewsComponent implements OnInit {
  channelMessages = channelMessages;

  constructor() { }

  ngOnInit(): void {
  }

}
