import { Component, OnInit } from '@angular/core';
import { Input } from '@angular/core';

@Component({
  selector: 'app-searchbar',
  templateUrl: './searchbar.component.html',
  styleUrls: ['./searchbar.component.styl']
})
export class SearchbarComponent implements OnInit {
  @Input() placeholder: string = "输入搜索内容";

  constructor() { }

  ngOnInit() {
  }

  search() {}
}
