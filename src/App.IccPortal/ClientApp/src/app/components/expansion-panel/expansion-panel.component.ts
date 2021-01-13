import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-expansion-panel',
  templateUrl: './expansion-panel.component.html',
  styleUrls: ['./expansion-panel.component.scss']
})
export class ExpansionPanelComponent implements OnInit {
  @Input() panelTitle: string;
  @Input() expanded: Boolean = false;

  constructor() { }

  ngOnInit(): void {
  }

  toggleExpand() {
    this.expanded = !this.expanded;
  }
}

