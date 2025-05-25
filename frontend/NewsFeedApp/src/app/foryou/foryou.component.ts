import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';

@Component({
  selector: 'app-foryou',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './foryou.component.html',
  styleUrls: ['./foryou.component.css']
})
export class ForyouComponent implements OnChanges {
  @Input() newsList: TileData[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['newsList']) {
      console.log('ForyouComponent detected newsList change:', this.newsList);
    }
  }
}
