import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-recrutamento-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './recrutamento-page.html',
})
export class RecrutamentoPage {
  @Input({ required: true }) vm!: any;
}
