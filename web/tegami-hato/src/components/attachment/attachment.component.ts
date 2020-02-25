import { Component, OnInit } from '@angular/core';
import { Input } from '@angular/core';

@Component({
  selector: 'app-attachment',
  templateUrl: './attachment.component.html',
  styleUrls: ['./attachment.component.styl']
})
export class AttachmentComponent implements OnInit {
  @Input() attachment;
  constructor() { }

  fileSize(size: number): string {
    if (size < 1000) {
      return size + ' B';
    } else if ((size /= 1024) < 1000) {
      return size.toFixed(2) + ' KiB';
    } else if ((size /= 1024) < 1000) {
      return size.toFixed(2) + ' MiB';
    } else {
      return size.toFixed(2) + ' GiB';
    }
  }

  ngOnInit(): void {
  }

}

// {
//   attachmentId: '01E1BWRVGEV4XD68NJVERG2ZXV',
//   filename: 'Konachan.com.png',
//   url: 'https://karenia-space-nano.sfo2.digitaloceanspaces.com/att/Konachan.com%20-%20299935%202980%20bow%20close%20gradient%20gray_hair%20higuchi_kaede%20long_hair%20nijisanji%20ponytail%20purple_eyes%20school_uniform%20tie%20waifu2x(RGB)(noise_scale)(Level1)(height%202560)_01E1BWRVGEV4XD68NJVERG2ZXV.png',
//   contentType: 'image/jpeg',
//   size: 3012528,
//   isAvailable: true
// }