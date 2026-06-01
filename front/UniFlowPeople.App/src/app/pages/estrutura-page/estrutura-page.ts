import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-estrutura-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './estrutura-page.html',
})
export class EstruturaPage {
  @Input({ required: true }) vm!: any;
}
