import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-beneficios-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './beneficios-page.html',
})
export class BeneficiosPage {
  @Input({ required: true }) vm!: any;
}
