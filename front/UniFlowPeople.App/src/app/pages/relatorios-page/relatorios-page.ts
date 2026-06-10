import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-relatorios-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './relatorios-page.html',
})
export class RelatoriosPage {
  @Input({ required: true }) vm!: any;
}
