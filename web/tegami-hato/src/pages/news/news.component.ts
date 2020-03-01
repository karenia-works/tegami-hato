import { Component, OnInit } from '@angular/core';
import { channelNews } from '../../assets/sampleData';

@Component({
  selector: 'app-news',
  templateUrl: './news.component.html',
  styleUrls: ['./news.component.styl']
})
export class NewsComponent implements OnInit {
  channelMessages = channelNews;

  constructor() { }

  ngOnInit(): void {
  }

}
