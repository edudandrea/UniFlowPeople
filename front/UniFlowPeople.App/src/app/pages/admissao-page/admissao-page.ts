import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admissao-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admissao-page.html',
})
export class AdmissaoPage {
  @Input({ required: true }) vm!: any;
}
