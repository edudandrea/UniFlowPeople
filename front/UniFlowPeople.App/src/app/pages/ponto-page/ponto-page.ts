import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-ponto-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ponto-page.html',
})
export class PontoPage {
  @Input({ required: true }) vm!: any;
}
